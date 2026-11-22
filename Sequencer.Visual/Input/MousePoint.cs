using Avalonia;

namespace Sequencer.Visual.Input
{
    public readonly struct MousePoint : IMousePoint
    {
        public MousePoint(Point point)
        {
            Point = point;
        }

        public MousePoint(double x, double y) : this(new Point(x, y))
        {
        }

        public double X => Point.X;

        public double Y => Point.Y;

        public Point Point { get; }
    }
}
