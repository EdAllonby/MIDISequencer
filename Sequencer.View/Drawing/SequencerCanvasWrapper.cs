using System.Windows;
using System.Windows.Controls;
using JetBrains.Annotations;

namespace Sequencer.View.Drawing
{
    public class SequencerCanvasWrapper : ISequencerCanvasWrapper
    {
        [NotNull] private readonly Canvas canvas;

        public SequencerCanvasWrapper([NotNull] Canvas canvas)
        {
            this.canvas = canvas;
        }

        public void AddChild(UIElement child)
        {
            canvas.Children.Add(child);
        }

        public void RemoveChild(UIElement child)
        {
            canvas.Children.Remove(child);
        }

        public double Width => canvas.ActualWidth;

        public double Height => canvas.ActualHeight;
    }
}