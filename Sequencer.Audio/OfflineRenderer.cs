using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using NAudio.Wave;
using Sequencer.Domain;
using Sequencer.Domain.Arrangement;
using Sequencer.Domain.Settings;
using Sequencer.Midi;

namespace Sequencer.Audio
{
    /// <summary>
    /// Renders a song to samples as fast as possible, through the same player as live playback, with a
    /// simulated clock instead of a real one. For exporting audio and for measuring the mix.
    /// </summary>
    public sealed class OfflineRenderer
    {
        private const int TicksPerQuarterNote = 96;

        [NotNull] private readonly IFrequencyCalculator frequencyCalculator;
        [NotNull] private readonly ITickCalculator tickCalculator;

        public OfflineRenderer([NotNull] IFrequencyCalculator frequencyCalculator, [NotNull] ITickCalculator tickCalculator)
        {
            this.frequencyCalculator = frequencyCalculator;
            this.tickCalculator = tickCalculator;
        }

        /// <summary>
        /// The limiter's activity during the last render (see <see cref="LimiterSampleProvider.Activity"/>).
        /// </summary>
        public double LimiterActivity { get; private set; }

        /// <summary>
        /// The loudest the mix was before the limiter, during the last render.
        /// </summary>
        public float PeakBeforeLimiter { get; private set; }

        /// <summary>
        /// Renders the whole song (song mode) at the given tempo, plus a short tail so the last notes fade out.
        /// </summary>
        [NotNull]
        public float[] Render([NotNull] Song song, int beatsPerMinute, [CanBeNull] MixerSettings mixer = null, double tailSeconds = 1.0)
        {
            mixer = mixer ?? new MixerSettings();
            var clock = new ManualClock();
            var output = new CaptureOutput();
            var state = new ArrangementState { Mode = PlaybackMode.Song };

            using (var player = new SequencerPlayer(new SignalProviderFactory(frequencyCalculator), song, state, clock, tickCalculator, mixer, output))
            {
                int totalTicks = song.LengthInSections * TicksPerQuarterNote * 16;
                double samplesPerTick = mixer.SampleRate * 60.0 / beatsPerMinute / TicksPerQuarterNote;
                var samples = new List<float>((int) (totalTicks * samplesPerTick) + mixer.SampleRate);
                var buffer = new float[512];
                double due = 0;

                clock.Start();

                for (var tick = 0; tick < totalTicks; tick++)
                {
                    clock.RaiseTick(tick);
                    due += samplesPerTick;
                    Pull(output.Source, samples, (int) Math.Floor(due) - samples.Count, buffer);
                }

                // Let the last notes release (ticking on past the end would loop back to the start).
                clock.Stop();
                Pull(output.Source, samples, (int) (tailSeconds * mixer.SampleRate), buffer);
                LimiterActivity = player.LimiterActivity;
                PeakBeforeLimiter = player.PeakBeforeLimiter;
                return samples.ToArray();
            }
        }

        private static void Pull([NotNull] ISampleProvider source, [NotNull] List<float> into, int count, [NotNull] float[] buffer)
        {
            while (count > 0)
            {
                int chunk = Math.Min(count, buffer.Length);
                source.Read(buffer.AsSpan(0, chunk));

                for (var i = 0; i < chunk; i++)
                {
                    into.Add(buffer[i]);
                }

                count -= chunk;
            }
        }

        private sealed class CaptureOutput : IAudioOutput
        {
            public ISampleProvider Source { get; private set; }

            public void Play(ISampleProvider source) => Source = source;

            public void Dispose()
            {
            }
        }

        /// <summary>
        /// A clock that ticks only when told to.
        /// </summary>
        private sealed class ManualClock : ISequencerClock
        {
            public int Ticks { get; private set; }

            public int LoopLengthTicks { get; set; } = int.MaxValue;

            public double BeatsPerMinute { get; set; } = 120;

            public int TicksPerQuarterNote => OfflineRenderer.TicksPerQuarterNote;

            public event EventHandler Started;

            public event EventHandler Stopped;

            public event EventHandler<TickEventArgs> Tick;

            public void Start() => Started?.Invoke(this, EventArgs.Empty);

            public void Continue() => Start();

            public void Stop() => Stopped?.Invoke(this, EventArgs.Empty);

            public void Pause() => Stop();

            public void RaiseTick(int tick)
            {
                Ticks = tick;
                Tick?.Invoke(this, new TickEventArgs(tick));
            }
        }
    }
}
