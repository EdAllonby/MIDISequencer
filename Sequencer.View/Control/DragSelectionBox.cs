using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using JetBrains.Annotations;
using Sequencer.Visual;
using Sequencer.Visual.Input;

namespace Sequencer.View.Control
{
    /// <summary>
    /// The rubber-band rectangle drawn while dragging to select notes.
    /// </summary>
    public sealed class DragSelectionBox : Canvas
    {
        private const double DragThreshold = 5;

        [NotNull] private readonly Border border = new Border
        {
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(2)
        };

        private bool isLeftMouseButtonDown;
        [NotNull] private IMousePoint originMouseDownPoint = new MousePoint();

        public DragSelectionBox()
        {
            System.Drawing.Color selection = new Domain.Settings.SequencerSettings().SelectionColour;
            border.BorderBrush = new SolidColorBrush(selection.ToAvalonia(), 0.9);
            border.Background = new SolidColorBrush(selection.ToAvalonia(), 0.12);
            Children.Add(border);
            IsVisible = false;
            IsHitTestVisible = false;
        }

        public bool IsDragging { get; private set; }

        public void SetNewOriginMousePosition([NotNull] IMousePoint mouseDownPoint)
        {
            originMouseDownPoint = mouseDownPoint;
            isLeftMouseButtonDown = true;
        }

        public void UpdateDragSelectionBox([NotNull] IMousePoint newPosition)
        {
            if (IsDragging)
            {
                UpdateDragSelectionRectSize(originMouseDownPoint, newPosition);
            }
            else if (isLeftMouseButtonDown)
            {
                Vector dragDelta = newPosition.Point - originMouseDownPoint.Point;

                if (dragDelta.Length > DragThreshold)
                {
                    IsDragging = true;
                    UpdateDragSelectionRectSize(originMouseDownPoint, newPosition);
                    IsVisible = true;
                }
            }
        }

        [NotNull]
        [ItemNotNull]
        public IEnumerable<TElement> FindMatches<TElement>([NotNull] [ItemNotNull] IEnumerable<TElement> possibleMatches)
            where TElement : IPositionAware
        {
            if (!IsDragging)
            {
                return Enumerable.Empty<TElement>();
            }

            Rect selectionBox = GetSelectionBox();
            return possibleMatches.Where(element => element.IntersectsWith(selectionBox)).ToList();
        }

        public void CloseSelectionBox()
        {
            IsDragging = false;
            IsVisible = false;
            isLeftMouseButtonDown = false;
        }

        private void UpdateDragSelectionRectSize([NotNull] IMousePoint pt1, [NotNull] IMousePoint pt2)
        {
            double x = Math.Max(0, Math.Min(pt1.X, pt2.X));
            double y = Math.Max(0, Math.Min(pt1.Y, pt2.Y));
            double width = Math.Max(pt1.X, pt2.X) - x;
            double height = Math.Max(pt1.Y, pt2.Y) - y;

            SetLeft(border, x);
            SetTop(border, y);
            border.Width = width;
            border.Height = height;
        }

        private Rect GetSelectionBox()
        {
            return new Rect(GetLeft(border), GetTop(border), border.Width, border.Height);
        }
    }
}
