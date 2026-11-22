using System;
using System.Drawing;
using Avalonia;
using Sequencer.Utilities;
using Point = Avalonia.Point;

namespace Sequencer.View.Control.RadialContextMenu
{
    /// <summary>
    /// Angle maths for the radial menu. Angles are in degrees, measured clockwise from the positive X axis
    /// (screen coordinates, Y pointing down).
    /// </summary>
    public static class RadialMenuGeometry
    {
        /// <summary>
        /// Cursor movement smaller than this, in pixels, selects nothing.
        /// </summary>
        public const double DeadZoneRadius = 4;

        public static bool IsCursorInSegment(Point centre, Point cursor, double startAngle, double sweepAngle)
        {
            double dx = cursor.X - centre.X;
            double dy = cursor.Y - centre.Y;

            if (Math.Sqrt(dx * dx + dy * dy) < DeadZoneRadius)
            {
                return false;
            }

            double cursorAngle = Math.Atan2(dy, dx) * 180.0 / Math.PI;
            double offset = ((cursorAngle - startAngle) % 360 + 360) % 360;

            return offset < sweepAngle;
        }

        public static Point PolarToCartesian(Point origin, double radius, double degrees)
        {
            PointF point = MathsUtilities.PolarToCartesian(new PointF((float) origin.X, (float) origin.Y), radius, degrees);
            return new Point(point.X, point.Y);
        }
    }
}
