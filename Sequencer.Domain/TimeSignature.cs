using JetBrains.Annotations;

namespace Sequencer.Domain
{
    /// <summary>
    /// Holds the musical details for a time signature.
    /// </summary>
    public sealed class TimeSignature
    {
        /// <summary>
        /// Create a new time signature container.
        /// </summary>
        /// <param name="beatsPerBar">How many beats are contained in a musical bar.</param>
        /// <param name="barsPerMeasure">How many bars are contained in a musical measure.</param>
        public TimeSignature(int beatsPerBar, int barsPerMeasure)
        {
            BeatsPerBar = beatsPerBar;
            BarsPerMeasure = barsPerMeasure;
        }

        /// <summary>
        /// How many beats are contained in a musical bar.
        /// </summary>
        public int BeatsPerBar { get; }

        /// <summary>
        /// How many bars are contained in a musical measure.
        /// </summary>
        public int BarsPerMeasure { get; }

        /// <summary>
        /// Calculated value of the total beats per a measure using the current <see cref="TimeSignature" /> values.
        /// </summary>
        public int BeatsPerMeasure => BeatsPerBar*BarsPerMeasure;

        [NotNull]
        public static TimeSignature FourFour => new TimeSignature(4, 4);
    }
}