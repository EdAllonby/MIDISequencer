using System;

namespace Sequencer.Midi
{
    public interface ISequencerClock
    {
        int Tempo { get; }
        int Ticks { get; set; }

        void Start();
        void Continue();
        void Stop();

        event EventHandler Started;
        event EventHandler Tick;
        event EventHandler Stopped;
        event EventHandler Continued;
    }
}