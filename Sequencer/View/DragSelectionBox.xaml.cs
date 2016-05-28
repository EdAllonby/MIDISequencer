using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using JetBrains.Annotations;

namespace Sequencer.View
{
    /// <summary>
    /// Interaction logic for DragSelectionBox.xaml
    /// </summary>
    public partial class DragSelectionBox
    {
        /// <summary>
        /// The threshold distance the mouse-cursor must move before drag-selection begins.
        /// </summary>
        private const double DragThreshold = 5;

        /// <summary>
        /// Set to 'true' when the left mouse-button is down.
        /// </summary>
        private bool isLeftMouseButtonDownOnWindow;

        /// <summary>
        /// Records the location of the mouse (relative to the window) when the left-mouse button has pressed down.
        /// </summary>
        private Point origMouseDownPoint;

        public DragSelectionBox()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Set to 'true' when dragging the 'selection rectangle'.
        /// Dragging of the selection rectangle only starts when the left mouse-button is held down and the mouse-cursor
        /// is moved more than a threshold distance.
        /// </summary>
        public bool IsDragging { get; private set; }

        /// <summary>
        /// Create a new selection box starting position.
        /// </summary>
        /// <param name="mouseDownPoint">The point to start the selection box.</param>
        public void SetNewOriginMousePosition(Point mouseDownPoint)
        {
            CaptureMouse();

            origMouseDownPoint = mouseDownPoint;

            isLeftMouseButtonDownOnWindow = true;
        }

        /// <summary>
        /// Update the position of the selection box.
        /// </summary>
        /// <param name="newPosition">The new position to update the selection box dimensions with.</param>
        public void UpdateDragSelectionBox(Point newPosition)
        {
            if (IsDragging)
            {
                UpdateDragSelectionRectSize(origMouseDownPoint, newPosition);
            }
            else if (isLeftMouseButtonDownOnWindow)
            {
                Vector dragDelta = newPosition - origMouseDownPoint;
                double dragDistance = Math.Abs(dragDelta.Length);
                if (dragDistance > DragThreshold)
                {
                    IsDragging = true;

                    InitDragSelectionRect(origMouseDownPoint, newPosition);
                }
            }
        }

        /// <summary>
        /// Find all <see cref="IPositionAware" /> elements inside the drag selection box.
        /// </summary>
        /// <typeparam name="TElement">Elements which are <see cref="IPositionAware" />.</typeparam>
        /// <param name="possibleMatches"><see cref="IPositionAware" /> elements to check are inside selection.</param>
        /// <returns>A subset of <see cref="possibleMatches" /> inside the selection.</returns>
        public IEnumerable<TElement> FindMatches<TElement>([NotNull] IEnumerable<TElement> possibleMatches)
            where TElement : IPositionAware
        {
            List<TElement> matches = new List<TElement>();
            if (IsDragging)
            {
                Rect selectionBox = GetSelectionBox();
                var selectionBoxGeometry = new RectangleGeometry(selectionBox);
                matches.AddRange(possibleMatches.Where(element => element.IntersectsWith(selectionBoxGeometry)));
            }

            return matches;
        }

        /// <summary>
        /// RemoveWithFade the selection box.
        /// </summary>
        public void CloseSelectionBox()
        {
            if (IsDragging)
            {
                IsDragging = false;
                Visibility = Visibility.Collapsed;
            }

            if (isLeftMouseButtonDownOnWindow)
            {
                isLeftMouseButtonDownOnWindow = false;
                ReleaseMouseCapture();
            }
        }

        /// <summary>
        /// Update the position and size of the rectangle used for drag selection.
        /// </summary>
        private void UpdateDragSelectionRectSize(Point pt1, Point pt2)
        {
            double x;
            double y;
            double width;
            double height;

            if (pt2.X < pt1.X)
            {
                x = Math.Max(0, pt2.X);
                width = pt1.X - x;
            }
            else
            {
                x = pt1.X;
                width = pt2.X - x;
            }

            if (pt2.Y < pt1.Y)
            {
                y = Math.Max(0, pt2.Y);
                height = pt1.Y - y;
            }
            else
            {
                y = pt1.Y;
                height = pt2.Y - y;
            }

            SetLeft(DragSelectionBorder, x);
            SetTop(DragSelectionBorder, y);
            DragSelectionBorder.Width = width;
            DragSelectionBorder.Height = height;
        }

        /// <summary>
        /// Initialize the rectangle used for drag selection.
        /// </summary>
        private void InitDragSelectionRect(Point pt1, Point pt2)
        {
            UpdateDragSelectionRectSize(pt1, pt2);

            Visibility = Visibility.Visible;
        }

        private Rect GetSelectionBox()
        {
            double x = GetLeft(DragSelectionBorder);
            double y = GetTop(DragSelectionBorder);
            double width = DragSelectionBorder.Width;
            double height = DragSelectionBorder.Height;

            return new Rect(x, y, width, height);
        }
    }
}