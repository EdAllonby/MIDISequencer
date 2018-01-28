using System;
using JetBrains.Annotations;
using Sanford.Multimedia.Midi;

namespace Sequencer.Midi
{
    public class InternalClock : IInternalClock
    {
        [NotNull] private readonly MidiInternalClock internalClockImplementation = new MidiInternalClock();

        public void Dispose()
        {
            internalClockImplementation.Dispose();
        }

        public event EventHandler Tick
        {
            add => internalClockImplementation.Tick += value;
            remove => internalClockImplementation.Tick -= value;
        }

        public int Tempo => internalClockImplementation.Tempo;

        public int Ticks
        {
            get => internalClockImplementation.Ticks;
            set => internalClockImplementation.SetTicks(value);
        }

        public int Ppqn
        {
            get => internalClockImplementation.Ppqn;
            set => internalClockImplementation.Ppqn = value;
        }

        public void Continue()
        {
            internalClockImplementation.Continue();
        }

        public void Stop()
        {
            internalClockImplementation.Stop();
        }

        public event EventHandler Started
        {
            add => internalClockImplementation.Started += value;
            remove => internalClockImplementation.Started -= value;
        }

        public event EventHandler Stopped
        {
            add => internalClockImplementation.Stopped += value;
            remove => internalClockImplementation.Stopped -= value;
        }

        public void Start()
        {
            internalClockImplementation.Start();
        }

        public void SetTicks(int value)
        {
            internalClockImplementation.SetTicks(value);
        }
    }
}