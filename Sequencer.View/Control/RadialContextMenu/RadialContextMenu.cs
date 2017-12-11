using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using JetBrains.Annotations;
using Sequencer.Domain;
using Sequencer.Utilities;
using Sequencer.ViewModel;
using Sequencer.Visual.Input;

namespace Sequencer.View.Control.RadialContextMenu
{
    public class RadialContextMenu<TMenuItem> : Canvas where TMenuItem : VisualEnumerableType<TMenuItem>
    {
        /// <summary>
        /// The Items selected Dependency Property.
        /// </summary>
        [NotNull] public static readonly DependencyProperty SelectedMenuItemProperty =
            DependencyProperty.Register(nameof(SelectedMenuItem), typeof(EnumerableType<TMenuItem>), typeof(RadialContextMenu<TMenuItem>),
                new FrameworkPropertyMetadata(null));

        /// <summary>
        /// The Items selected Dependency Property.
        /// </summary>
        [NotNull] public static readonly DependencyProperty MenuRadiusProperty =
            DependencyProperty.Register(nameof(MenuRadius), typeof(double), typeof(RadialContextMenu<TMenuItem>),
                new FrameworkPropertyMetadata(null));

        [NotNull] [ItemNotNull] private readonly List<Line> menuSeperators = new List<Line>();
        [NotNull] [ItemNotNull] private readonly List<ContextMenuSegment<TMenuItem>> segments = new List<ContextMenuSegment<TMenuItem>>();

        private IMousePoint centre;
        private bool isActive;

        [CanBeNull] private RadialCursorLine radialCursorLine;


        public EnumerableType<TMenuItem> SelectedMenuItem
        {
            get => (EnumerableType<TMenuItem>) GetValue(SelectedMenuItemProperty);
            set => SetValue(SelectedMenuItemProperty, value);
        }

        public double MenuRadius
        {
            get
            {
                object value = GetValue(MenuRadiusProperty);
                if (value == null)
                {
                    return 0;
                }

                return (double) value;
            }
            set => SetValue(MenuRadiusProperty, value);
        }

        private static double AngleSize => (double) 360 / EnumerableType<TMenuItem>.Count;

        public void SetCursorPosition([NotNull] IMousePoint point)
        {
            if (!isActive)
            {
                return;
            }

            radialCursorLine?.NewEndPoint(point);

            foreach (ContextMenuSegment<TMenuItem> contextMenuSegment in segments)
            {
                var segment = new LineGeometry(centre?.Point ?? new Point(), point.Point);

                if (contextMenuSegment.IntersectsWith(segment))
                {
                    contextMenuSegment.Select();
                }
                else
                {
                    contextMenuSegment.Unselect();
                }
            }
        }

        public void BuildPopup([NotNull] IMousePoint point)
        {
            isActive = true;
            centre = point;
            radialCursorLine = new RadialCursorLine(this, point);

            foreach (TMenuItem menuItem in EnumerableType<TMenuItem>.All)
            {
                double angle = AngleSize / 2 + AngleSize * menuItem.Value;

                // We want to start calculating where the azimuth is on the Y axis.
                // So, we tranlate all angles by -90 degrees to rotate from X to Y.
                double startAngle = angle - 90;
                Point lineEndPoint = MathsUtilities.PolarToRectangular(point.Point, MenuRadius, startAngle);

                Line seperatorLine = CreateSeperatorLine(point.Point, lineEndPoint);

                segments.Add(new ContextMenuSegment<TMenuItem>(menuItem, point.Point, MenuRadius, startAngle, AngleSize));

                menuSeperators.Add(seperatorLine);
            }

            AddElementsToCanvas(segments);
            AddElementsToCanvas(menuSeperators);
        }

        public void ClosePopup()
        {
            TMenuItem selectedMenuItem = segments.FirstOrDefault(item => item.IsSelected)?.MenuItem;

            if (selectedMenuItem != null)
            {
                SelectedMenuItem = selectedMenuItem;
            }

            DoubleAnimation animation = DrawingUtilities.CreateFadeOutAnimation(100);

            animation.Completed += (s, a) => AfterRemoveAnimation();

            foreach (Line menuSeperator in menuSeperators)
            {
                menuSeperator.BeginAnimation(OpacityProperty, animation);
            }

            foreach (ContextMenuSegment<TMenuItem> segment in segments)
            {
                segment.BeginAnimation(OpacityProperty, animation);
            }

            radialCursorLine?.RemoveWithFade(OpacityProperty);

            isActive = false;
        }

        private void AddElementsToCanvas([NotNull] [ItemNotNull] IEnumerable<FrameworkElement> elements)
        {
            foreach (FrameworkElement element in elements)
            {
                if (!Children.Contains(element))
                {
                    Children.Add(element);
                }
            }
        }

        private void RemoveElementToCanvas([NotNull] [ItemNotNull] IEnumerable<FrameworkElement> elements)
        {
            foreach (FrameworkElement element in elements.ToList())
            {
                if (Children.Contains(element))
                {
                    Children.Remove(element);
                }
            }
        }

        private void AfterRemoveAnimation()
        {
            RemoveElementToCanvas(menuSeperators);
            RemoveElementToCanvas(segments);

            menuSeperators.Clear();
            segments.Clear();
        }

        private static Line CreateSeperatorLine(Point point, Point lineEndPoint)
        {
            return new Line
            {
                StrokeThickness = 1,
                Stroke = Brushes.DimGray,
                X1 = point.X,
                Y1 = point.Y,
                X2 = lineEndPoint.X,
                Y2 = lineEndPoint.Y
            };
        }
    }
}