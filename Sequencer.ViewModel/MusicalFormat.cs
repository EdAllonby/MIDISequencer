using System;
using JetBrains.Annotations;
using Sequencer.Domain;

namespace Sequencer.ViewModel
{
    /// <summary>
    /// Positions and lengths the way musicians read them.
    /// </summary>
    public static class MusicalFormat
    {
        public const int SixteenthsPerBar = 16;

        /// <summary>
        /// "bar.beat.sixteenth", each counted from 1. The start of the sequence is "1.1.1".
        /// </summary>
        [NotNull]
        public static string Position([NotNull] IPosition position, [NotNull] TimeSignature timeSignature, int ticksPerQuarterNote)
        {
            int bar = (position.Measure - 1) * timeSignature.BarsPerMeasure + position.Bar;
            int sixteenth = position.Ticks / (ticksPerQuarterNote / 4) + 1;
            return $"{bar}.{position.Beat}.{sixteenth}";
        }

        /// <summary>
        /// A length in sixteenth notes as a musical fraction: 1/16, 1/8, 3/16, 1/4 ... 1 bar, 2 bars.
        /// </summary>
        [NotNull]
        public static string Length(int sixteenths)
        {
            if (sixteenths <= 0)
            {
                return "0";
            }

            int bars = sixteenths / SixteenthsPerBar;
            int remainder = sixteenths % SixteenthsPerBar;
            string barText = bars == 1 ? "1 bar" : $"{bars} bars";

            if (remainder == 0)
            {
                return barText;
            }

            int divisor = GreatestCommonDivisor(remainder, SixteenthsPerBar);
            string fraction = $"{remainder / divisor}/{SixteenthsPerBar / divisor}";
            return bars == 0 ? fraction : $"{barText} + {fraction}";
        }

        private static int GreatestCommonDivisor(int a, int b)
        {
            while (b != 0)
            {
                (a, b) = (b, a % b);
            }

            return Math.Abs(a);
        }
    }
}
