using System;

namespace Sequencer.Midi
{
    public interface ISequencerClock
    {
        int Ticks { get; }

        /// <summary>
        /// How many ticks play before the clock loops back to 0: one section, or the whole song.
        /// </summary>
        int LoopLengthTicks { get; set; }

        double BeatsPerMinute { get; set; }

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