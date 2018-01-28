using System;
using System.Windows;
using JetBrains.Annotations;

namespace Sequencer.Utilities
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
        [Pure]
        public static T Clamp<T>([NotNull] this T value, [NotNull] T minimum, [NotNull] T maximum) where T : IComparable<T>
        {
            return value.CompareTo(minimum) < 0 ? minimum : (value.CompareTo(maximum) > 0 ? maximum : value);
        }

        /// <summary>
        /// Convert from polar coordinates to cartesian coordinates.
        /// Only works for points to the left of the origin.
        /// </summary>
        /// <param name="origin">The starting coordinate of the polar coordinates.</param>
        /// <param name="radius">The radius of the point in pixels.</param>
        /// <param name="degrees">The angle of the point in radians.</param>
        /// <returns>The point in cartesian coordinates.</returns>
        [Pure]
        public static Point PolarToCartesian(Point origin, double radius, double degrees)
        {
            double degreesInRadians = ToRadians(degrees);

            double x = origin.X + radius * Math.Cos(degreesInRadians);
            double y = origin.Y + radius * Math.Sin(degreesInRadians);

            return new Point(x, y);
        }

        /// <summary>
        /// Finds the nearest value based on a multiple
        /// </summary>
        /// <param name="value">The value to calculate nearest multiple.</param>
        /// <param name="multiple">The multiple used in the calculation.</param>
        /// <returns></returns>
        [Pure]
        public static int NearestValue(double value, int multiple)
        {
            if (multiple <= 0)
            {
                throw new ArgumentException("multiple must be greater than 0");
            }

            double remainder = value % multiple;

            double result = value - remainder;

            if (remainder >= multiple / 2.0)
            {
                result += multiple;
            }

            return (int) result;
        }

        [Pure]
        private static double ToRadians(double degrees)
        {
            return degrees * Math.PI / 180.0;
        }
    }
}