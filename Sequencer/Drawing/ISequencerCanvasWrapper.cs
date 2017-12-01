using System.Windows;
using JetBrains.Annotations;

namespace Sequencer.Drawing
{
    public interface ISequencerCanvasWrapper
    {
        void AddChild([NotNull] UIElement child);
        void RemoveChild([NotNull] UIElement child);

        double Height { get; }

    }
}