using System;

namespace Sequencer.Midi
{
    internal interface ISequencerClock
    {
        int Tempo { get; }
        void SetTicks(int ticks);
        void Start();
        void Continue();
        void Stop();

        event EventHandler Started;
        event EventHandler Tick;
        event EventHandler Stopped;
        event EventHandler Continued;
    }
}