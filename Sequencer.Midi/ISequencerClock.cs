﻿using System;

namespace Sequencer.Midi
{
    public interface ISequencerClock
    {
        int Ticks { get; }

        int TicksPerQuarterNote { get; }

        void Start();
        void Continue();
        void Stop();
        void Pause();

        event EventHandler Started;
        event EventHandler Stopped;
        event EventHandler<TickEventArgs> Tick;
    }
}