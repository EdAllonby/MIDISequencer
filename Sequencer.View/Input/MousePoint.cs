using System.Windows;

namespace Sequencer.View.Input
{
    public class MousePoint : IMousePoint
    {
        public MousePoint(Point point)
        {
            Point = point;
        }

        public double X => Point.X;

        public double Y => Point.Y;
        public Point Point { get; }
    }
}