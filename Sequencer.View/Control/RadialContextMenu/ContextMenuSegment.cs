using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Media;
using JetBrains.Annotations;
using Sequencer.ViewModel;

namespace Sequencer.View.Control.RadialContextMenu
{
    /// <summary>
    /// One pie slice of the radial menu, with the item's icon in the middle.
    /// </summary>
    public sealed class ContextMenuSegment<TMenuItem> : Canvas
        where TMenuItem : VisualEnumerableType<TMenuItem>
    {
        [NotNull] private static readonly IBrush SliceBrush = new SolidColorBrush(Color.Parse("#2A2D33"), 0.97);
        [NotNull] private static readonly IBrush SelectedSliceBrush = new SolidColorBrush(Color.Parse("#E8A33D"));
        [NotNull] private static readonly IBrush IconBrush = new SolidColorBrush(Color.Parse("#C9CCD1"));
        [NotNull] private static readonly IBrush SelectedIconBrush = new SolidColorBrush(Color.Parse("#16171A"));

        [NotNull] private readonly Path segmentShape = new Path();
        [CanBeNull] private readonly Path icon;
        private readonly Point centre;
        private readonly double startAngle;
        private readonly double sweepAngle;

        public ContextMenuSegment([NotNull] TMenuItem menuItem, Point centre, double menuRadius, double startAngle, double sweepAngle)
        {
            MenuItem = menuItem;
            this.centre = centre;
            this.startAngle = startAngle;
            this.sweepAngle = sweepAngle;

            // The slice is the centre, a line out to the start of the arc, and the arc itself.
            double endAngle = startAngle + sweepAngle;
            Point arcStart = RadialMenuGeometry.PolarToCartesian(centre, menuRadius, startAngle);
            Point arcEnd = RadialMenuGeometry.PolarToCartesian(centre, menuRadius, endAngle);

            var figure = new PathFigure
            {
                StartPoint = centre,
                IsClosed = true,
                Segments = new PathSegments
                {
                    new LineSegment { Point = arcStart },
                    new ArcSegment
                    {
                        Point = arcEnd,
                        Size = new Size(menuRadius, menuRadius),
                        IsLargeArc = sweepAngle >= 180.0,
                        SweepDirection = SweepDirection.Clockwise
                    }
                }
            };

            segmentShape.Data = new PathGeometry { Figures = new PathFigures { figure } };
            Children.Add(segmentShape);

            Geometry iconGeometry = Icons.ForTool(menuItem.IconName);

            if (iconGeometry != null)
            {
                const double iconSize = 18;
                icon = new Path
                {
                    Data = iconGeometry,
                    Width = iconSize,
                    Height = iconSize,
                    Stretch = Stretch.Uniform,
                    StrokeThickness = 1.6,
                    StrokeLineCap = PenLineCap.Round,
                    StrokeJoin = PenLineJoin.Round
                };

                Point iconCentre = RadialMenuGeometry.PolarToCartesian(centre, menuRadius * 0.58, endAngle - sweepAngle / 2);
                SetLeft(icon, iconCentre.X - iconSize / 2);
                SetTop(icon, iconCentre.Y - iconSize / 2);
                Children.Add(icon);
            }

            Unselect();
        }

        [NotNull]
        public TMenuItem MenuItem { get; }

        public bool IsSelected { get; private set; }

        public bool ContainsDirectionTo(Point cursor)
        {
            return RadialMenuGeometry.IsCursorInSegment(centre, cursor, startAngle, sweepAngle);
        }

        public void Select()
        {
            segmentShape.Fill = SelectedSliceBrush;

            if (icon != null)
            {
                icon.Stroke = SelectedIconBrush;
            }

            IsSelected = true;
        }

        public void Unselect()
        {
            segmentShape.Fill = SliceBrush;

            if (icon != null)
            {
                icon.Stroke = IconBrush;
            }

            IsSelected = false;
        }
    }
}
