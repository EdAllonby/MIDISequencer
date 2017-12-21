using System;

namespace Sequencer.Midi
{
    public sealed class TickEventArgs : EventArgs
    {
        public TickEventArgs(int currentTick)
        {
            CurrentTick = currentTick;
        }

        public int CurrentTick { get; }
    }
}