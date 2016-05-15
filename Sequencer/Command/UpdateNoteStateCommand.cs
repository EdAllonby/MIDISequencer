using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using JetBrains.Annotations;
using log4net;
using Sequencer.Domain;

namespace Sequencer.Command
{
    public sealed class UpdateNoteStateCommand
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(UpdateNoteStateCommand));
        private readonly NoteState newState;

        private readonly List<VisualNote> sequencerNotes;

        public UpdateNoteStateCommand([NotNull] List<VisualNote> sequencerNotes, NoteState newState)
        {
            this.sequencerNotes = sequencerNotes;
            this.newState = newState;
        }

        public void Execute(IEnumerable<VisualNote> notesToChange)
        {
            foreach (VisualNote visualNote in notesToChange)
            {
                if ((visualNote != null) && (visualNote.NoteState != newState))
                {
                    visualNote.NoteState = newState;

                    Log.Info($"Visual note {visualNote} has been {visualNote.NoteState}");
                }
            }

            if (!Keyboard.IsKeyDown(Key.LeftCtrl))
            {
                foreach (VisualNote visualNote in sequencerNotes.Except(notesToChange))
                {
                    if (visualNote.NoteState != NoteState.Unselected)
                    {
                        Log.Info($"Visual note {visualNote} has been {visualNote.NoteState}");

                        visualNote.NoteState = NoteState.Unselected;
                    }
                }
            }
        }
    }
}