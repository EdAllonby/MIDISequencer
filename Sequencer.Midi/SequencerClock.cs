using System;
using JetBrains.Annotations;
using Sanford.Multimedia.Midi;

namespace Sequencer.Midi
{
    /// <summary>
    /// Currently just a wrapper for the <see cref="MidiInternalClock" />.
    /// </summary>
    public class SequencerClock : ISequencerClock
    {
        private readonly int totalTicks;
        [NotNull] private readonly MidiInternalClock clock = new MidiInternalClock();

        public int Tempo => clock.Tempo;

        public SequencerClock()
        {
            int bpm = 500000;

            clock = new MidiInternalClock();
            // TODO: Inject setting
            const int sequencerSize = 32 / 2;
            totalTicks = sequencerSize * TicksPerQuarterNote;
            clock.Tick += OnTick;
        }

        private void OnTick(object sender, EventArgs e)
        {
            if (Ticks == totalTicks)
            {
                clock.SetTicks(0);
                clock.Start();
            }

            Tick?.Invoke(this, e);
        }

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

            Tick?.Invoke(this, EventArgs.Empty);
        }

        public void Pause()
        {
            clock.Stop();
            Tick?.Invoke(this, EventArgs.Empty);
        }

        public int TicksPerQuarterNote => clock.Ppqn;

        public event EventHandler Started
        {
            add => clock.Started += value;
            remove => clock.Started += value;
        }

        public event EventHandler Tick;
    }
}