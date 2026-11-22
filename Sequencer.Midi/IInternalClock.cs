using System;

namespace Sequencer.Midi
{
    public interface IInternalClock : IDisposable
    {
        /// <summary>
        /// Microseconds per quarter note.
        /// </summary>
        int Tempo { get; set; }
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