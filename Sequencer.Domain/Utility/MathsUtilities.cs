using System;

namespace Sequencer.Domain.Utility
{
    public static class MathsUtilities
    {
        /// <summary>
        /// Clamps a comparable type between a minimum and a maximum.
        /// </summary>
        /// <typeparam name="T">The comparable type.</typeparam>
        /// <param name="value">The value to clamp.</param>
        /// <param name="minimum">The minimum the value can be.</param>
        /// <param name="maximum">The maximum the value can be.</param>
        /// <returns>The clamped value between the minimum and maximum.</returns>
        public static T Clamp<T>(this T value, T minimum, T maximum) where T : IComparable<T>
        {
            return value.CompareTo(minimum) < 0 ? minimum : (value.CompareTo(maximum) > 0 ? maximum : value);
        }
    }
}