using System.Windows;
using System.Windows.Controls;

namespace Sequencer.Drawing
{
    public class SequencerCanvasWrapper : ISequencerCanvasWrapper
    {
        private readonly Canvas canvas;

        public SequencerCanvasWrapper(Canvas canvas)
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

        public double Height => canvas.ActualHeight;
    }
}