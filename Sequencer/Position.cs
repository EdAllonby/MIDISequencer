using System;

namespace Sequencer
{
    /// <summary>
    /// Represents a musical position.
    /// </summary>
    public sealed class Position
    {
        /// <summary>
        /// A position based on current measure, bar and beat.
        /// </summary>
        /// <param name="measure">The measure the position is in.</param>
        /// <param name="bar">The bar the position is in.</param>
        /// <param name="beat">The beat the position is in.</param>
        public Position(int measure, int bar, int beat)
        {
            Measure = measure;
            Bar = bar;
            Beat = beat;
        }

        public int Measure { get; }

        public int Bar { get; }

        public int Beat { get; }

        /// <summary>
        /// Gets the sum of beats for the <see cref="Position" />.
        /// </summary>
        /// <param name="timeSignature">The <see cref="TimeSignature" /> to use when calculating the sum of beats.</param>
        /// <returns></returns>
        public int SummedBeat(TimeSignature timeSignature)
        {
            return ((Measure - 1)*timeSignature.BeatsPerMeasure) + ((Bar - 1)*timeSignature.BeatsPerBar) + Beat;
        }

        /// <summary>
        /// Get the next <see cref="Position" />.
        /// </summary>
        /// <param name="timeSignature">The <see cref="TimeSignature" /> to use to calculate the next <see cref="Position" />.</param>
        /// <returns>The next <see cref="Position" />.</returns>
        public Position NextPosition(TimeSignature timeSignature)
        {
            int totalBeats = SummedBeat(timeSignature);
            return PositionFromBeat(totalBeats + 1, timeSignature);
        }

        /// <summary>
        /// Get the position from origin of a sum of beats.
        /// </summary>
        /// <param name="totalBeats">The summed beats to the particular position.</param>
        /// <param name="timeSignature">The <see cref="TimeSignature" /> to use in calculation.</param>
        /// <returns>The position from origin of the summed beats.</returns>
        public static Position PositionFromBeat(int totalBeats, TimeSignature timeSignature)
        {
            int measures = 1 + (totalBeats - 1)/timeSignature.BeatsPerMeasure;
            int remainingBeatsForBars = totalBeats - (timeSignature.BeatsPerMeasure*(measures - 1));
            int bars = 1 + (remainingBeatsForBars - 1)/timeSignature.BeatsPerBar;
            int remainingBeats = remainingBeatsForBars - (timeSignature.BeatsPerBar*(bars - 1));

            return new Position(measures, bars, remainingBeats);
        }

        public override string ToString()
        {
            return $"Position at Measure {Measure}, Bar {Bar}, Beat {Beat}";
        }
    }
}