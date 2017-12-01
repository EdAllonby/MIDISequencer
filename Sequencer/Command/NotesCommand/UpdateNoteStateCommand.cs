using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using JetBrains.Annotations;
using log4net;
using Sequencer.Input;
using Sequencer.Utilities;
using Sequencer.View;

namespace Sequencer.Command.NotesCommand
{
    public sealed class UpdateNoteStateCommand : INotesCommand
    {
        [NotNull] private static readonly ILog Log = LogExtensions.GetLoggerSafe(typeof(UpdateNoteStateCommand));
        private readonly NoteState newState;

        [NotNull] private readonly ISequencerNotes sequencerNotes;
        [NotNull] private readonly IKeyboardStateProcessor keyboardStateProcessor;

        public UpdateNoteStateCommand([NotNull] ISequencerNotes sequencerNotes, [NotNull] IKeyboardStateProcessor keyboardStateProcessor, NoteState newState)
        {
            this.sequencerNotes = sequencerNotes;
            this.keyboardStateProcessor = keyboardStateProcessor;
            this.newState = newState;
        }

        public void Execute(IEnumerable<IVisualNote> notesToChange)
        {
            IEnumerable<IVisualNote> notesList = notesToChange as IList<IVisualNote> ?? notesToChange.ToList();

            foreach (IVisualNote visualNote in notesList)
            {
                if ((visualNote != null) && (visualNote.NoteState != newState))
                {
                    visualNote.NoteState = newState;

                    Log.Info($"Visual note {visualNote} has been {visualNote.NoteState}");
                }
            }

            if (!keyboardStateProcessor.IsKeyDown(Key.LeftCtrl))
            {
                foreach (IVisualNote visualNote in sequencerNotes.FindAllOtherNotes(notesList))
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