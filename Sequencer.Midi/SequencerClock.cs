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

        public int Ticks
        {
            get => clock.Ticks;
            set => clock.SetTicks(value);
        }


        public void Start()
        {
            clock.Start();
        }

        public void Continue()
        {
            clock.Continue();
        }

        public void Stop()
        {
            clock.Stop();
        }

        public int TicksPerQuarterNote => clock.Ppqn;

        public event EventHandler Started
        {
            add => clock.Started += value;
            remove => clock.Started += value;
        }

        public event EventHandler Tick
        {
            add => clock.Tick += value;
            remove => clock.Tick += value;
        }

        public event EventHandler Stopped
        {
            add => clock.Stopped += value;
            remove => clock.Stopped += value;
        }

        public event EventHandler Continued
        {
            add => clock.Continued += value;
            remove => clock.Continued += value;
        }
    }
}