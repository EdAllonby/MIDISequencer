using JetBrains.Annotations;
using Sequencer.Domain;

namespace Sequencer.View
{
    public interface IVisualNoteFactory
    {
        [NotNull]
        IVisualNote CreateNote([NotNull] Pitch pitch, [NotNull] IPosition start, [NotNull] IPosition end);
    }
}