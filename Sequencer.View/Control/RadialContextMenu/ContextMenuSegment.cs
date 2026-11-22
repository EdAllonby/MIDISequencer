using System;
using System.Collections.Generic;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
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
        [NotNull] private static readonly Dictionary<string, Bitmap> IconCache = new Dictionary<string, Bitmap>();
        [NotNull] private static readonly IBrush SelectedBrush = new SolidColorBrush(Colors.CornflowerBlue);
        [NotNull] private static readonly IBrush UnselectedBrush = new SolidColorBrush(Colors.LightSteelBlue, 0.7);

        [NotNull] private readonly Path segmentShape = new Path();
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

            var geometry = new PathGeometry { Figures = new PathFigures { figure } };
            segmentShape.Data = geometry;
            Children.Add(segmentShape);

            Bitmap icon = LoadIcon(menuItem.IconName);

            if (icon != null)
            {
                double iconSize = Math.Min(geometry.Bounds.Width, geometry.Bounds.Height) * 0.7;
                var image = new Image { Source = icon, Stretch = Stretch.Fill, Width = iconSize, Height = iconSize };
                RenderOptions.SetBitmapInterpolationMode(image, Avalonia.Media.Imaging.BitmapInterpolationMode.HighQuality);

                Point iconCentre = RadialMenuGeometry.PolarToCartesian(centre, menuRadius * 0.5, endAngle - sweepAngle / 2);
                SetLeft(image, iconCentre.X - iconSize / 2);
                SetTop(image, iconCentre.Y - iconSize / 2);
                Children.Add(image);
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
            segmentShape.Fill = SelectedBrush;
            IsSelected = true;
        }

        public void Unselect()
        {
            segmentShape.Fill = UnselectedBrush;
            IsSelected = false;
        }

        [CanBeNull]
        private static Bitmap LoadIcon([CanBeNull] string iconName)
        {
            if (iconName == null)
            {
                return null;
            }

            if (!IconCache.TryGetValue(iconName, out Bitmap bitmap))
            {
                using (var stream = AssetLoader.Open(new Uri($"avares://Sequencer/Assets/{iconName}.png")))
                {
                    bitmap = new Bitmap(stream);
                }

                IconCache[iconName] = bitmap;
            }

            return bitmap;
        }
    }
}
