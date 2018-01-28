using System;

namespace Sequencer.Midi
{
    public interface IInternalClock : IDisposable
    {
        event EventHandler Tick;
        int Tempo { get; }
        int Ticks { get; set; }
        int Ppqn { get; set; }
        void Continue();
        void Stop();
        event EventHandler Started;
        event EventHandler Stopped;
        void Start();
    }
}