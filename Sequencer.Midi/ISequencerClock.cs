using System;

namespace Sequencer.Midi
{
    public interface ISequencerClock
    {
        int Tempo { get; }
        int Ticks { get; }

        int TicksPerQuarterNote { get; }

        void Start();
        void Continue();
        void Stop();
        void Pause();


        event EventHandler Started;
        event EventHandler Tick;
    }
}