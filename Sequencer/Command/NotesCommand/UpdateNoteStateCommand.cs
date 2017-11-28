using System.Collections.Generic;
using System.Windows.Input;
using JetBrains.Annotations;
using log4net;
using Sequencer.View;

namespace Sequencer.Command.NotesCommand
{
    public sealed class UpdateNoteStateCommand : INotesCommand
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(UpdateNoteStateCommand));
        private readonly NoteState newState;

        private readonly ISequencerNotes sequencerNotes;

        public UpdateNoteStateCommand([NotNull] ISequencerNotes sequencerNotes, NoteState newState)
        {
            this.sequencerNotes = sequencerNotes;
            this.newState = newState;
        }

        public void Execute(IEnumerable<IVisualNote> notesToChange)
        {
            foreach (IVisualNote visualNote in notesToChange)
            {
                if ((visualNote != null) && (visualNote.NoteState != newState))
                {
                    visualNote.NoteState = newState;

                    Log.Info($"Visual note {visualNote} has been {visualNote.NoteState}");
                }
            }

            if (!Keyboard.IsKeyDown(Key.LeftCtrl))
            {
                foreach (IVisualNote visualNote in sequencerNotes.FindAllOtherNotes(notesToChange))
                {
                    if (visualNote.NoteState != NoteState.Unselected)
                    {
                        Log.Info($"Visual note {visualNote} has been {visualNote.NoteState}");

                        visualNote.NoteState = NoteState.Unselected;
                    }
                }
            }

            sequencerNotes.NoteStateChanged();
        }
    }
}