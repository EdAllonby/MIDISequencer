using System.Collections.Generic;
using JetBrains.Annotations;
using Sequencer.Visual;

namespace Sequencer.View.Command.NotesCommand
{
    public sealed class DeleteNotesCommand : IDeleteNotesCommand, INotesCommand
    {
        [NotNull] private readonly ISequencerNotes sequencerNotes;

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