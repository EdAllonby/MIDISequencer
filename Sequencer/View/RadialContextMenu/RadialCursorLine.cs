using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using JetBrains.Annotations;

namespace Sequencer.View.RadialContextMenu
{
    /// <summary>
    /// A visual representation of a line with a fixed start point which follows a cursor for its end point.
    /// </summary>
    public class RadialCursorLine
    {
        private const int FadeOutDuration = 200;
        private readonly Line cursorLine;
        private readonly Canvas ownerCanvas;

        /// <summary>
        /// Create a cursor line with a fixed starting point to be displayed on a <see cref="Canvas" />.
        /// </summary>
        /// <param name="canvas">The canvas to display this cursor line on.</param>
        /// <param name="startingPosition">The fixed starting point of this cursor line.</param>
        public RadialCursorLine([NotNull] Canvas canvas, Point startingPosition)
        {
            ownerCanvas = canvas;

            cursorLine = new Line
            {
                Visibility = Visibility.Visible,
                StrokeThickness = 1,
                Stroke = Brushes.Black,
                X1 = startingPosition.X,
                Y1 = startingPosition.Y
            };

            canvas.Children.Add(cursorLine);
            Panel.SetZIndex(cursorLine, 99);
        }

        /// <summary>
        /// Give the cursor line a new end point.
        /// </summary>
        /// <param name="endPosition">The new end point.</param>
        public void NewEndPoint(Point endPosition)
        {
            cursorLine.X2 = endPosition.X;
            cursorLine.Y2 = endPosition.Y;
        }

        /// <summary>
        /// Remove a cursor from the canvas with a fade effect.
        /// </summary>
        /// <param name="opacityProperty">Used for animating the removal.</param>
        public void RemoveWithFade(DependencyProperty opacityProperty)
        {
            DoubleAnimation animation = DrawingUtilities.CreateFadeOutAnimation(FadeOutDuration);

            animation.Completed += (s, a) => AfterRemoveAnimation();

            cursorLine.BeginAnimation(opacityProperty, animation);
        }

        private void AfterRemoveAnimation()
        {
            cursorLine.Opacity = 0;
            ownerCanvas.Children.Remove(cursorLine);
        }
    }
}