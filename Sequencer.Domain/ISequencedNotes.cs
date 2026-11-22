using System.Collections.Generic;
using JetBrains.Annotations;

namespace Sequencer.Domain
{
    /// <summary>
    /// The notes in the sequence, as seen by playback.
    /// </summary>
    public interface ISequencedNotes
    {
        [NotNull]
        [ItemNotNull]
        IEnumerable<ISequencedNote> AllNotes { get; }

        [NotNull]
        [ItemNotNull]
        IEnumerable<ISequencedNote> FindNotesFromStartingPosition([NotNull] IPosition position);

        [NotNull]
        [ItemNotNull]
        IEnumerable<ISequencedNote> FindNotesFromEndingPosition([NotNull] IPosition position);
    }
}
