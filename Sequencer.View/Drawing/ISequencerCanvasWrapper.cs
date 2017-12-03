using System.Windows;
using JetBrains.Annotations;

namespace Sequencer.View.Drawing
{
    public interface ISequencerCanvasWrapper
    {
        double Width { get; }

        double Height { get; }
        void AddChild([NotNull] UIElement child);
        void RemoveChild([NotNull] UIElement child);
    }
}