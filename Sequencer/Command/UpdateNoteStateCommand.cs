﻿using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using JetBrains.Annotations;
using log4net;

namespace Sequencer.Command
{
    public sealed class UpdateNoteStateCommand
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(UpdateNoteStateCommand));

        private readonly List<VisualNote> sequencerNotes;
        private readonly NoteState noteState;

        public UpdateNoteStateCommand([NotNull] List<VisualNote> sequencerNotes, NoteState noteState)
        {
            this.sequencerNotes = sequencerNotes;
            this.noteState = noteState;
        }

        public void Execute(IEnumerable<VisualNote> notesToChange)
        {
            foreach (VisualNote visualNote in notesToChange)
            {
                if ((visualNote != null) && (visualNote.NoteState != noteState))
                {
                    visualNote.NoteState = noteState;

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
