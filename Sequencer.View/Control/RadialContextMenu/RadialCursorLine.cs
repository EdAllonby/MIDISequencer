using System;
using Avalonia;
using Avalonia.Animation;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Media;
using Avalonia.Threading;
using JetBrains.Annotations;
using Sequencer.Visual.Input;

namespace Sequencer.View.Control.RadialContextMenu
{
    /// <summary>
    /// The line from the menu's centre to the cursor.
    /// </summary>
    public class RadialCursorLine
    {
        private static readonly TimeSpan FadeOutDuration = TimeSpan.FromMilliseconds(200);

        [NotNull] private readonly Line cursorLine;
        [NotNull] private readonly Canvas ownerCanvas;

        public RadialCursorLine([NotNull] Canvas canvas, [NotNull] IMousePoint startingPosition)
        {
            ownerCanvas = canvas;
            cursorLine = new Line
            {
                StrokeThickness = 1,
                Stroke = Brushes.Black,
                StartPoint = startingPosition.Point,
                EndPoint = startingPosition.Point,
                ZIndex = 99,
                Transitions = new Transitions { new DoubleTransition { Property = Avalonia.Visual.OpacityProperty, Duration = FadeOutDuration } }
            };

            canvas.Children.Add(cursorLine);
        }

        public void NewEndPoint([NotNull] IMousePoint endPosition)
        {
            cursorLine.EndPoint = endPosition.Point;
        }

        public void RemoveWithFade()
        {
            cursorLine.Opacity = 0;
            DispatcherTimer.RunOnce(() => ownerCanvas.Children.Remove(cursorLine), FadeOutDuration);
        }
    }
}
