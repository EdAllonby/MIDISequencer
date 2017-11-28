using System.Windows;

namespace Sequencer.Drawing
{
    public interface ISequencerCanvasWrapper
    {
        void AddChild(UIElement child);
        void RemoveChild(UIElement child);

        double Height { get; }

    }
}