using System;
using System.Windows;

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
        public static T Clamp<T>(this T value, T minimum, T maximum) where T : IComparable<T>
        {
            return value.CompareTo(minimum) < 0 ? minimum : (value.CompareTo(maximum) > 0 ? maximum : value);
        }

        /// <summary>
        /// Convert from polar coordinates to rectangular coordinates.
        /// Only works for points to the left of the origin.
        /// </summary>
        /// <param name="origin">The starting coordinate of the polar coordinates.</param>
        /// <param name="radius">The radius of the point in pixels.</param>
        /// <param name="degrees">The angle of the point in radians.</param>
        /// <returns>The point in rectangular coordinates.</returns>
        public static Point PolarToRectangular(Point origin, double radius, double degrees)
        {
            try
            {
                double x = origin.X + GetRectangularLength(radius, degrees);
                double y = origin.Y + GetRectangularHeight(radius, degrees);

                return new Point(x, y);
            }
            catch (OverflowException ex)
            {
                ex.Data.Add("Screen polar Radius", radius);
                ex.Data.Add("Screen polar Theta", degrees);
                throw;
            }
        }

        public static double GetRectangularHeight(double radius, double degrees)
        {
            return radius*Math.Sin(ToRadians(degrees));
        }

        public static double GetRectangularLength(double radius, double degrees)
        {
            return radius*Math.Cos(ToRadians(degrees));
        }

        private static double ToRadians(double degrees)
        {
            return (degrees*Math.PI)/180.0;
        }
    }
}