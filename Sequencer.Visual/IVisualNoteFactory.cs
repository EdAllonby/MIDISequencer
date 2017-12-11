using JetBrains.Annotations;
using Sequencer.Domain;

namespace Sequencer.Visual
{
    public interface IVisualNoteFactory
    {
        [NotNull]
        IVisualNote CreateNote([NotNull] Pitch pitch, [NotNull] IPosition start, [NotNull] IPosition end);
    }
}