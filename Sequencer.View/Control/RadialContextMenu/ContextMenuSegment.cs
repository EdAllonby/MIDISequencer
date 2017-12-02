using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using JetBrains.Annotations;
using Sequencer.Utilities;
using Sequencer.ViewModel;

namespace Sequencer.View.Control.RadialContextMenu
{
    /// <summary>
    /// A context menu segment.
    /// </summary>
    /// <typeparam name="TMenuItem">The menu item to represent.</typeparam>
    public sealed class ContextMenuSegment<TMenuItem> : Canvas, IPositionAware
        where TMenuItem : VisualEnumerableType<TMenuItem>
    {
        [NotNull] private readonly Path segmentShape = new Path();
        private readonly Color selectedColor = Colors.CornflowerBlue;
        private readonly Color unselectedColor = Colors.LightSteelBlue;

        /// <summary>
        /// Creates a visual context menu segment.
        /// </summary>
        /// <param name="menuItem">The item to draw.</param>
        /// <param name="startingPoint">The starting position.</param>
        /// <param name="menuRadius">The radius of the menu.</param>
        /// <param name="startAngle">The starting angle to start drawing.</param>
        /// <param name="angle">The size of the segment in degrees.</param>
        public ContextMenuSegment([NotNull] TMenuItem menuItem, Point startingPoint, double menuRadius, double startAngle, double angle)
        {
            // This uses 3 points to create the segment. The starting point, the beginning of the arc point, and the end of the arc point.
            // This will create the segment geometry we can use to draw the shape.

            MenuItem = menuItem;
            double endAngle = startAngle + angle;

            // path container
            var pathData = new PathFigure
            {
                StartPoint = startingPoint,
                IsClosed = true
            };

            //  start angle line
            Point arcStartPosition = MathsUtilities.PolarToRectangular(startingPoint, menuRadius, startAngle);

            var lineToArc = new LineSegment {Point = arcStartPosition};

            pathData.Segments?.Add(lineToArc);

            // outer arc
            Point arcEndPosition = MathsUtilities.PolarToRectangular(startingPoint, menuRadius, endAngle);

            var arcSize = new Size(menuRadius, menuRadius);

            var arc = new ArcSegment
            {
                IsLargeArc = angle >= 180.0,
                Point = arcEndPosition,
                Size = arcSize,
                SweepDirection = SweepDirection.Clockwise
            };

            pathData.Segments?.Add(arc);

            var pathGeometry = new PathGeometry
            {
                Figures = {pathData}
            };

            segmentShape.Data = pathGeometry;
            Children.Add(segmentShape);

            double segmentIconSize = Math.Min(pathGeometry.Bounds.Width, pathGeometry.Bounds.Height);
            var segmentIcon = new Image
            {
                Source = MenuItem.Visual.ToBitmapImage(),
                Stretch = Stretch.Fill,
                Height = segmentIconSize*0.7,
                Width = segmentIconSize*0.7
            };

            // We don't want the scaled bitmap to look pixelated,
            // and we don't really care about the performance of a context menu.
            // This will set the icon's scaling logic to the best scaling technique.
            RenderOptions.SetBitmapScalingMode(segmentIcon, BitmapScalingMode.Fant);

            Children.Add(segmentIcon);

            double midAngle = endAngle - (angle/2);

            Point imagePosition = MathsUtilities.PolarToRectangular(startingPoint, menuRadius*0.5, midAngle);

            segmentIcon.SetCentreOnCanvas(imagePosition);

            Unselect();
        }

        /// <summary>
        /// The <see cref="TMenuItem" /> this segment represents.
        /// </summary>
        [NotNull] public TMenuItem MenuItem { get; }

        /// <summary>
        /// Whether this segment is selected.
        /// </summary>
        public bool IsSelected { get; private set; }

        /// <summary>
        /// Determines if this segment intersects with other geometry.
        /// </summary>
        /// <param name="geometry">The other geometry to determine if it intersects.</param>
        /// <returns>Whether this segment intersects with the other geometry.</returns>
        public bool IntersectsWith(Geometry geometry)
        {
            return segmentShape.Data?.FillContainsWithDetail(geometry) != IntersectionDetail.Empty;
        }

        /// <summary>
        /// Select this segment.
        /// </summary>
        public void Select()
        {
            segmentShape.Fill = new SolidColorBrush(selectedColor);
            IsSelected = true;
        }

        /// <summary>
        /// Unselect this segment.
        /// </summary>
        public void Unselect()
        {
            segmentShape.Fill = new SolidColorBrush(unselectedColor) {Opacity = 0.7};
            IsSelected = false;
        }
    }
}