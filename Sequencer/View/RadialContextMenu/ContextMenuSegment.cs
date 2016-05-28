using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using Sequencer.Utilities;
using Sequencer.ViewModel;

namespace Sequencer.View.RadialContextMenu
{
    public sealed class ContextMenuSegment<TMenuItem> : Canvas, IPositionAware where TMenuItem : VisualEnumerableType<TMenuItem>
    {
        private readonly Color selectedColor = Colors.CornflowerBlue;
        private readonly Path shape = new Path();
        private readonly Color unselectedColor = Colors.LightSteelBlue;

        public ContextMenuSegment(TMenuItem menuItem, Point startingPoint, double menuRadius, double startAngle, double angle)
        {
            MenuItem = menuItem;
            double endAngle = startAngle + angle;

            // path container
            var figureP = new Point(menuRadius, menuRadius);
            var pathData = new PathFigure
            {
                StartPoint = figureP,
                IsClosed = true
            };

            //  start angle line
            double lineX = menuRadius + MathsUtilities.GetRectangularHeight(menuRadius, startAngle);
            double lineY = menuRadius - MathsUtilities.GetRectangularLength(menuRadius, startAngle);
            var line = new LineSegment {Point = new Point(lineX, lineY)};
            pathData.Segments.Add(line);

            // outer arc
            double arcX = menuRadius + MathsUtilities.GetRectangularHeight(menuRadius, endAngle);
            double arcY = menuRadius - MathsUtilities.GetRectangularLength(menuRadius, endAngle);
            var arcP = new Point(arcX, arcY);

            var arcS = new Size(menuRadius, menuRadius);

            var arc = new ArcSegment
            {
                IsLargeArc = angle >= 180.0,
                Point = arcP,
                Size = arcS,
                SweepDirection = SweepDirection.Clockwise
            };

            pathData.Segments.Add(arc);

            var pathGeometry = new PathGeometry
            {
                Figures = {pathData},
                Transform = new TranslateTransform(startingPoint.X - menuRadius, startingPoint.Y - menuRadius)
            };

            shape.Data = pathGeometry;
            Children.Add(shape);

            double imageSize = Math.Min(pathGeometry.Bounds.Width, pathGeometry.Bounds.Height);
            var menuItemVisual = new Image
            {
                Source = MenuItem.Visual.ToBitmapImage(),
                Stretch = Stretch.Fill,
                Height = imageSize*0.7,
                Width = imageSize*0.7
            };

            RenderOptions.SetBitmapScalingMode(menuItemVisual, BitmapScalingMode.Fant);

            double imageX = MathsUtilities.GetRectangularHeight(menuRadius*0.5, startAngle + (angle/2));
            double imageY = -MathsUtilities.GetRectangularLength(menuRadius*0.5, startAngle + (angle/2));

            Children.Add(menuItemVisual);
            SetLeft(menuItemVisual, imageX + (startingPoint.X - (menuItemVisual.Width/2)));
            SetTop(menuItemVisual, imageY + (startingPoint.Y - (menuItemVisual.Height/2)));

            Unselect();
        }

        public TMenuItem MenuItem { get; }

        public bool IsSelected { get; private set; }

        public bool IntersectsWith(Geometry geometry)
        {
            return shape.Data.FillContainsWithDetail(geometry) != IntersectionDetail.Empty;
        }

        public void Select()
        {
            shape.Fill = new SolidColorBrush(selectedColor);
            IsSelected = true;
        }

        public void Unselect()
        {
            shape.Fill = new SolidColorBrush(unselectedColor) {Opacity = 0.7};
            IsSelected = false;
        }
    }
}