using System;
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
        /// <param name="ticks">The sub ticks the position is in</param>
        public Position(int measure, int bar, int beat, int ticks = 0)
        {
            Measure = measure;
            Bar = bar;
            Beat = beat;
            Ticks = ticks;
        }

        public int Measure { get; }

        public int Bar { get; }

        public int Beat { get; }

        public int Ticks { get; }

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

            if (Measure == other.Measure && Bar == other.Bar && Beat == other.Beat && Ticks > other.Ticks)
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

        public int TotalTicks(TimeSignature timeSignature, int ticksPerQuarterNote)
        {
            int beatTicks = (SummedBeat(timeSignature) - 1) * ticksPerQuarterNote;

            return beatTicks + Ticks;
        }

        /// <summary>
        /// Get the next <see cref="IPosition" />.
        /// </summary>
        /// <param name="resolution">The next position using the current note resolution.</param>
        /// <param name="timeSignature">The <see cref="TimeSignature" /> to use to calculate the next <see cref="IPosition" />.</param>
        /// <param name="ticksPerQuarterNote">Ticks per quarter note.</param>
        /// <returns>The next <see cref="IPosition" />.</returns>
        public IPosition NextPosition(NoteResolution resolution, TimeSignature timeSignature, int ticksPerQuarterNote)
        {
            int tickDelta = NoteResolutionCalculator.GetTicksForResolution(resolution, ticksPerQuarterNote);
            return PositionRelativeByTicks(tickDelta, timeSignature, ticksPerQuarterNote);
        }

        /// <summary>
        /// Get the previous <see cref="IPosition" />.
        /// </summary>
        /// <param name="resolution">The previous position using the current note resolution.</param>
        /// <param name="timeSignature">The <see cref="TimeSignature" /> to use to calculate the previous <see cref="IPosition" />.</param>
        /// <param name="ticksPerQuarterNote">Ticks per quarter note.</param>
        /// <returns>The previous <see cref="IPosition" />.</returns>
        public IPosition PreviousPosition(NoteResolution resolution, TimeSignature timeSignature, int ticksPerQuarterNote)
        {
            int tickDelta = -1 * NoteResolutionCalculator.GetTicksForResolution(resolution, ticksPerQuarterNote);
            return PositionRelativeByTicks(tickDelta, timeSignature, ticksPerQuarterNote);
        }

        public IPosition PositionRelativeByTicks(int tickDelta, TimeSignature timeSignature, int ticksPerQuarterNote)
        {
            int totalTicks = TotalTicks(timeSignature, ticksPerQuarterNote);
            return PositionFromTick(totalTicks + tickDelta, timeSignature, ticksPerQuarterNote);
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
            if (other is null) return false;
            if (ReferenceEquals(this, other)) return true;
            return Measure == other.Measure && Bar == other.Bar && Beat == other.Beat && Ticks == other.Ticks;
        }

        /// <summary>
        /// Get the position from origin of a sum of beats.
        /// </summary>
        /// <param name="totalTicks">The summed ticks to the particular position.</param>
        /// <param name="timeSignature">The <see cref="TimeSignature" /> to use in calculation.</param>
        /// <param name="ticksPerQuarterNote">Ticks per quarter note.</param>
        /// <returns>The position from origin of the summed beats.</returns>
        [NotNull]
        public static IPosition PositionFromTick(int totalTicks, [NotNull] TimeSignature timeSignature, int ticksPerQuarterNote)
        {
            var totalBeats = (int) Math.Floor(totalTicks / (decimal) ticksPerQuarterNote) + 1;

            int measures = 1 + (totalBeats - 1) / timeSignature.BeatsPerMeasure;
            int remainingBeatsForBars = totalBeats - timeSignature.BeatsPerMeasure * (measures - 1);
            int bars = 1 + (remainingBeatsForBars - 1) / timeSignature.BeatsPerBar;
            int remainingBeats = remainingBeatsForBars - timeSignature.BeatsPerBar * (bars - 1);

            int remainingTicks = totalTicks - (totalBeats - 1) * ticksPerQuarterNote;

            return new Position(measures, bars, remainingBeats, remainingTicks);
        }

        public override bool Equals(object obj)
        {
            if (obj is null) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj is IPosition position && Equals(position);
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
            return $"Measure: {Measure}, Bar: {Bar}, Beat: {Beat}, Sub-Beat {Ticks}";
        }
    }
}