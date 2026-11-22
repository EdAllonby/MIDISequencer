using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia;
using Avalonia.Animation;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Data;
using Avalonia.Media;
using Avalonia.Threading;
using JetBrains.Annotations;
using Sequencer.Domain;
using Sequencer.ViewModel;
using Sequencer.Visual.Input;

namespace Sequencer.View.Control.RadialContextMenu
{
    /// <summary>
    /// A pie menu that opens around the cursor while the secondary button is held. Releasing over a slice
    /// selects that item.
    /// </summary>
    public class RadialContextMenu<TMenuItem> : Canvas where TMenuItem : VisualEnumerableType<TMenuItem>
    {
        public static readonly StyledProperty<TMenuItem> SelectedMenuItemProperty =
            AvaloniaProperty.Register<RadialContextMenu<TMenuItem>, TMenuItem>(nameof(SelectedMenuItem), defaultBindingMode: BindingMode.TwoWay);

        public static readonly StyledProperty<double> MenuRadiusProperty =
            AvaloniaProperty.Register<RadialContextMenu<TMenuItem>, double>(nameof(MenuRadius), 52);

        private static readonly TimeSpan FadeOutDuration = TimeSpan.FromMilliseconds(100);

        [NotNull] [ItemNotNull] private readonly List<Line> menuSeparators = new List<Line>();
        [NotNull] [ItemNotNull] private readonly List<ContextMenuSegment<TMenuItem>> segments = new List<ContextMenuSegment<TMenuItem>>();
        private bool isActive;
        [CanBeNull] private RadialCursorLine radialCursorLine;

        public RadialContextMenu()
        {
            // The menu only draws; the window routes pointer input to it.
            IsHitTestVisible = false;
        }

        public TMenuItem SelectedMenuItem
        {
            get => GetValue(SelectedMenuItemProperty);
            set => SetValue(SelectedMenuItemProperty, value);
        }

        public double MenuRadius
        {
            get => GetValue(MenuRadiusProperty);
            set => SetValue(MenuRadiusProperty, value);
        }

        private static double AngleSize => 360.0 / EnumerableType<TMenuItem>.Count(x => x.CanView);

        public bool IsOpen => isActive;

        public void SetCursorPosition([NotNull] IMousePoint point)
        {
            if (!isActive)
            {
                return;
            }

            radialCursorLine?.NewEndPoint(point);

            foreach (ContextMenuSegment<TMenuItem> segment in segments)
            {
                if (segment.ContainsDirectionTo(point.Point))
                {
                    segment.Select();
                }
                else
                {
                    segment.Unselect();
                }
            }
        }

        public void BuildPopup([NotNull] IMousePoint point)
        {
            isActive = true;
            radialCursorLine = new RadialCursorLine(this, point);

            foreach (TMenuItem menuItem in EnumerableType<TMenuItem>.All.Where(x => x.CanView))
            {
                double angle = AngleSize / 2 + AngleSize * menuItem.Value;

                // Rotate by -90 degrees so the first slice is measured from the vertical.
                double startAngle = angle - 90;
                Point lineEndPoint = RadialMenuGeometry.PolarToCartesian(point.Point, MenuRadius, startAngle);

                segments.Add(new ContextMenuSegment<TMenuItem>(menuItem, point.Point, MenuRadius, startAngle, AngleSize));
                menuSeparators.Add(CreateSeparatorLine(point.Point, lineEndPoint));
            }

            foreach (Avalonia.Controls.Control element in segments.Cast<Avalonia.Controls.Control>().Concat(menuSeparators))
            {
                element.Transitions = new Transitions { new DoubleTransition { Property = OpacityProperty, Duration = FadeOutDuration } };
                Children.Add(element);
            }
        }

        public void ClosePopup()
        {
            if (!isActive)
            {
                return;
            }

            TMenuItem selectedMenuItem = segments.FirstOrDefault(item => item.IsSelected)?.MenuItem;

            if (selectedMenuItem != null)
            {
                SelectedMenuItem = selectedMenuItem;
            }

            // Fade out, then remove exactly these elements, so a menu reopened during the fade is unaffected.
            List<Avalonia.Controls.Control> closing = segments.Cast<Avalonia.Controls.Control>().Concat(menuSeparators).ToList();

            foreach (Avalonia.Controls.Control element in closing)
            {
                element.Opacity = 0;
            }

            DispatcherTimer.RunOnce(() =>
            {
                foreach (Avalonia.Controls.Control element in closing)
                {
                    Children.Remove(element);
                }
            }, FadeOutDuration);

            radialCursorLine?.RemoveWithFade();
            radialCursorLine = null;
            segments.Clear();
            menuSeparators.Clear();
            isActive = false;
        }

        [NotNull]
        private static Line CreateSeparatorLine(Point centre, Point lineEndPoint)
        {
            return new Line
            {
                // Gaps between the slices, in the window colour.
                StrokeThickness = 2,
                Stroke = new SolidColorBrush(Color.Parse("#16171A")),
                StartPoint = centre,
                EndPoint = lineEndPoint
            };
        }
    }
}
