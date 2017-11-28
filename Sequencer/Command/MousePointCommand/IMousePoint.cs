﻿using System.Windows;

namespace Sequencer.Command.MousePointCommand
{
    public interface IMousePoint
    {
        double X { get; }
        double Y { get; }

        Point Point { get; }
    }

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