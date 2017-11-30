using System.Collections.Generic;
using JetBrains.Annotations;
using Sequencer.View;

namespace Sequencer.Command.NotesCommand
{
    public sealed class DeleteNotesCommand : IDeleteNotesCommand, INotesCommand
    {
        private readonly ISequencerNotes sequencerNotes;

        public DeleteNotesCommand([NotNull] ISequencerNotes sequencerNotes)
        {
            this.sequencerNotes = sequencerNotes;
        }

        public void Execute(IEnumerable<IVisualNote> notesToDelete)
        {
            foreach (IVisualNote noteToDelete in notesToDelete)
            {
                sequencerNotes.DeleteNote(noteToDelete);
            }
        }
    }
}