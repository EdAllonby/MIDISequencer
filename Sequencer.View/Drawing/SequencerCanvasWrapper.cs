using Avalonia.Controls;
using JetBrains.Annotations;
using Sequencer.Visual;

namespace Sequencer.View.Drawing
{
    public class SequencerCanvasWrapper : ISequencerCanvasWrapper
    {
        [NotNull] private readonly Canvas canvas;

        public SequencerCanvasWrapper([NotNull] Canvas canvas)
        {
            this.canvas = canvas;
        }

        public void AddChild(Avalonia.Controls.Control child)
        {
            canvas.Children.Add(child);
        }

        public void RemoveChild(Avalonia.Controls.Control child)
        {
            canvas.Children.Remove(child);
        }

        public double Width => canvas.Bounds.Width;

        public double Height => canvas.Bounds.Height;
    }
}