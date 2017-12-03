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
        [NotNull] private readonly MidiInternalClock clock = new MidiInternalClock();

        public int Tempo => clock.Tempo;

        public SequencerClock()
        {
            clock.Tick += OnTick;
        }

        private void OnTick(object sender, EventArgs e)
        {
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