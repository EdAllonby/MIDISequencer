using System.Collections.Generic;
using JetBrains.Annotations;
using Sequencer.View;

namespace Sequencer.Command.NotesCommand
{
    public sealed class DeleteNotesCommand : INotesCommand
    {
        private readonly SequencerNotes sequencerNotes;

        public DeleteNotesCommand([NotNull] SequencerNotes sequencerNotes)
        {
            this.sequencerNotes = sequencerNotes;
        }

        public void Execute([NotNull] IEnumerable<VisualNote> notesToDelete)
        {
            foreach (VisualNote noteToDelete in notesToDelete)
            {
                sequencerNotes.DeleteNote(noteToDelete);
            }
        }
    }
}