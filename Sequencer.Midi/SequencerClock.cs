using System;
using JetBrains.Annotations;
using Sequencer.Domain.Settings;

namespace Sequencer.Midi
{
    public class SequencerClock : ISequencerClock, IDisposable
    {
        [NotNull] private readonly IInternalClock clock;
        private readonly int totalTicks;

        public SequencerClock([NotNull] IMusicalSettings musicalSettings, [NotNull] IInternalClock internalClock)
        {
            clock = internalClock;

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

        public double BeatsPerMinute => 60000000.0 / MicrosecondsPerQuarterNote;

        private int MicrosecondsPerQuarterNote => clock.Tempo;

        public int Ticks => clock.Ticks;

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