using System;
using JetBrains.Annotations;
using Sanford.Multimedia.Midi;
using Sequencer.Domain.Settings;

namespace Sequencer.Midi
{
    /// <summary>
    /// Currently just a wrapper for the <see cref="MidiInternalClock" />.
    /// </summary>
    public class SequencerClock : ISequencerClock, IDisposable
    {
        [NotNull] private readonly MidiInternalClock clock = new MidiInternalClock();
        private readonly int totalTicks;

        public SequencerClock([NotNull] IMusicalSettings musicalSettings)
        {
            TicksPerQuarterNote = musicalSettings.TicksPerQuarterNote;

            int totalBeats = musicalSettings.TotalMeasures * musicalSettings.TimeSignature.BeatsPerMeasure;
            totalTicks = totalBeats * TicksPerQuarterNote;

            clock.Tick += OnTick;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public int Tempo => clock.Tempo;

        public int Ticks
        {
            get => clock.Ticks;
            set => clock.SetTicks(value);
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
            clock.SetTicks(0);

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
            remove => clock.Started += value;
        }

        public event EventHandler Stopped
        {
            add => clock.Stopped += value;
            remove => clock.Stopped += value;
        }

        public event EventHandler<TickEventArgs> Tick;

        private void OnTick(object sender, EventArgs e)
        {
            if (Ticks == totalTicks)
            {
                clock.SetTicks(0);
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