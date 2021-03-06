﻿using System.Windows;

namespace Sequencer.Visual.Input
{
    public struct MousePoint : IMousePoint
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