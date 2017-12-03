using JetBrains.Annotations;

namespace Sequencer.Domain
{
    /// <summary>
    /// Represents a musical position.
    /// </summary>
    public sealed class Position : IPosition
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

        public int CompareTo([NotNull] IPosition other)
        {
            if (Equals(other))
            {
                return 0;
            }
            if (Measure > other.Measure)
            {
                return 1;
            }
            if (Measure == other.Measure && Bar > other.Bar)
            {
                return 1;
            }
            if (Measure == other.Measure && Bar == other.Bar && Beat > other.Beat)
            {
                return 1;
            }

            return -1;
        }

        /// <summary>
        /// Gets the sum of beats for the <see cref="IPosition" />.
        /// </summary>
        /// <param name="timeSignature">The <see cref="TimeSignature" /> to use when calculating the sum of beats.</param>
        /// <returns></returns>
        public int SummedBeat(TimeSignature timeSignature)
        {
            return (Measure - 1) * timeSignature.BeatsPerMeasure + (Bar - 1) * timeSignature.BeatsPerBar + Beat;
        }

        /// <summary>
        /// Get the next <see cref="IPosition" />.
        /// </summary>
        /// <param name="timeSignature">The <see cref="TimeSignature" /> to use to calculate the next <see cref="IPosition" />.</param>
        /// <returns>The next <see cref="IPosition" />.</returns>
        public IPosition NextPosition(TimeSignature timeSignature)
        {
            return PositionRelativeByBeats(1, timeSignature);
        }

        /// <summary>
        /// Get the previous <see cref="IPosition" />.
        /// </summary>
        /// <param name="timeSignature">The <see cref="TimeSignature" /> to use to calculate the previous <see cref="IPosition" />.</param>
        /// <returns>The previous <see cref="IPosition" />.</returns>
        public IPosition PreviousPosition(TimeSignature timeSignature)
        {
            return PositionRelativeByBeats(-1, timeSignature);
        }

        public IPosition PositionRelativeByBeats(int beatDelta, TimeSignature timeSignature)
        {
            int totalBeats = SummedBeat(timeSignature);
            return PositionFromBeat(totalBeats + beatDelta, timeSignature);
        }

        public bool IsGreaterThan(IPosition other)
        {
            return CompareTo(other) > 0;
        }

        public bool IsGreaterThanOrEqual(IPosition other)
        {
            return Equals(this, other) || CompareTo(other) > 0;
        }

        public bool IsLessThan(IPosition other)
        {
            return CompareTo(other) < 0;
        }

        public bool IsLessThanOrEqual(IPosition other)
        {
            return Equals(this, other) || CompareTo(other) < 0;
        }

        public bool Equals(IPosition other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Measure == other.Measure && Bar == other.Bar && Beat == other.Beat;
        }

        /// <summary>
        /// Get the position from origin of a sum of beats.
        /// </summary>
        /// <param name="totalBeats">The summed beats to the particular position.</param>
        /// <param name="timeSignature">The <see cref="TimeSignature" /> to use in calculation.</param>
        /// <returns>The position from origin of the summed beats.</returns>
        [NotNull]
        public static IPosition PositionFromBeat(int totalBeats, [NotNull] TimeSignature timeSignature)
        {
            int measures = 1 + (totalBeats - 1) / timeSignature.BeatsPerMeasure;
            int remainingBeatsForBars = totalBeats - timeSignature.BeatsPerMeasure * (measures - 1);
            int bars = 1 + (remainingBeatsForBars - 1) / timeSignature.BeatsPerBar;
            int remainingBeats = remainingBeatsForBars - timeSignature.BeatsPerBar * (bars - 1);

            return new Position(measures, bars, remainingBeats);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj is IPosition && Equals((IPosition) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = Measure;
                hashCode = (hashCode * 397) ^ Bar;
                hashCode = (hashCode * 397) ^ Beat;
                return hashCode;
            }
        }

        public static bool operator <([NotNull] Position first, [NotNull] Position second)
        {
            return first.CompareTo(second) < 0;
        }

        public static bool operator >([NotNull] Position first, [NotNull] Position second)
        {
            return first.CompareTo(second) > 0;
        }

        public static bool operator <=([NotNull] Position first, [NotNull] Position second)
        {
            return Equals(first, second) || first.CompareTo(second) < 0;
        }

        public static bool operator >=([NotNull] Position first, [NotNull] Position second)
        {
            return Equals(first, second) || first.CompareTo(second) > 0;
        }

        public override string ToString()
        {
            return $"Measure: {Measure}, Bar: {Bar}, Beat: {Beat}";
        }
    }
}