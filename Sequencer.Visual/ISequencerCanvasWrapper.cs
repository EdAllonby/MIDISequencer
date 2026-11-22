using Avalonia.Controls;
using JetBrains.Annotations;

namespace Sequencer.Visual
{
    public interface ISequencerCanvasWrapper
    {
        double Width { get; }

        double Height { get; }

        void AddChild([NotNull] Control child);

        void RemoveChild([NotNull] Control child);
    }
}
