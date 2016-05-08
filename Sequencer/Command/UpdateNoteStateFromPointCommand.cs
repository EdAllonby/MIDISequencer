using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using JetBrains.Annotations;

namespace Sequencer.Command
{
    public sealed class UpdateNoteStateFromPointCommand : MousePointNoteCommand
    {
        public UpdateNoteStateFromPointCommand([NotNull] List<VisualNote> sequencerNotes, [NotNull] SequencerSettings sequencerSettings, [NotNull] SequencerDimensionsCalculator sequencerDimensionsCalculator)
            : base(sequencerNotes, sequencerSettings, sequencerDimensionsCalculator)
        {
        }

        public override void Execute(Point mousePoint)
        {
            VisualNote actionableNote = sequencerDimensionsCalculator.FindNoteFromPoint(sequencerNotes, mousePoint);

            if (actionableNote != null)
            {
                if (!Keyboard.IsKeyDown(Key.LeftCtrl))
                {
                    foreach (VisualNote visualNote in sequencerNotes.Where(x => x != actionableNote))
                    {
                        visualNote.NoteState = NoteState.Unselected;
                    }
                }

                actionableNote.NoteState = actionableNote.NoteState == NoteState.Selected ? NoteState.Unselected : NoteState.Selected;

                Log.Info($"Visual note {actionableNote} has been {actionableNote.NoteState}");
            }
        }
    }
}