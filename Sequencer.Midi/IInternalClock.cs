using System;

namespace Sequencer.Midi
{
    public interface IInternalClock : IDisposable
    {
        int Tempo { get; }
        int Ticks { get; set; }
        int Ppqn { get; set; }
        event EventHandler Tick;
        void Continue();
        void Stop();
        event EventHandler Started;
        event EventHandler Stopped;
        void Start();
    }
}