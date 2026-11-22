using System;
using JetBrains.Annotations;
using Sequencer.Domain.Settings;

namespace Sequencer.Midi
{
    public class SequencerClock : ISequencerClock, IDisposable
    {
        [NotNull] private readonly IInternalClock clock;
        private volatile int totalTicks;

        public SequencerClock([NotNull] IMusicalSettings musicalSettings, [NotNull] IInternalClock internalClock)
        {
            clock = internalClock;

            TicksPerQuarterNote = musicalSettings.TicksPerQuarterNote;

            int totalBeats = musicalSettings.TotalMeasures * musicalSettings.TimeSignature.BeatsPerMeasure;
            totalTicks = totalBeats * TicksPerQuarterNote;

            clock.Tick += OnTick;
        }

        public double BeatsPerMinute
        {
            get => 60000000.0 / MicrosecondsPerQuarterNote;
            set
            {
                if (value <= 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(value), "Beats per minute must be positive.");
                }

                clock.Tempo = (int) Math.Round(60000000.0 / value);
            }
        }

        private int MicrosecondsPerQuarterNote => clock.Tempo;

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public int Ticks => clock.Ticks;

        public int LoopLengthTicks
        {
            get => totalTicks;
            set
            {
                if (value <= 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(value), "The loop must be at least one tick long.");
                }

                totalTicks = value;
            }
        }

        public void Start()
        {
            clock.Continue();
        }

        public void Continue()
        {
            clock.Continue();
        }

        public void Stop()
        {
            clock.Stop();
            clock.Ticks = 0;

            Tick?.Invoke(this, new TickEventArgs(Ticks));
        }

        public void Pause()
        {
            clock.Stop();
            Tick?.Invoke(this, new TickEventArgs(Ticks));
        }

        public int TicksPerQuarterNote
        {
            get => clock.Ppqn;
            private set => clock.Ppqn = value;
        }

        public event EventHandler Started
        {
            add => clock.Started += value;
            remove => clock.Started -= value;
        }

        public event EventHandler Stopped
        {
            add => clock.Stopped += value;
            remove => clock.Stopped -= value;
        }

        public event EventHandler<TickEventArgs> Tick;

        private void OnTick(object sender, EventArgs e)
        {
            // At or past the end (the loop can shrink while playing), go back to the start.
            if (Ticks >= totalTicks)
            {
                clock.Ticks = 0;
                clock.Start();
            }

            Tick?.Invoke(this, new TickEventArgs(Ticks));
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                clock.Dispose();
            }
        }
    }
}