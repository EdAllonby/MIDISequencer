using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using JetBrains.Annotations;

namespace Sequencer.Command
{
    public sealed class UpdateNoteStateFromPointCommand : MousePointNoteCommand
    {
        private readonly UpdateNoteStateCommand noteStateSelectedCommand;
        private readonly UpdateNoteStateCommand noteStateUnselectedCommand;

        public UpdateNoteStateFromPointCommand([NotNull] List<VisualNote> sequencerNotes, [NotNull] SequencerSettings sequencerSettings, [NotNull] SequencerDimensionsCalculator sequencerDimensionsCalculator)
            : base(sequencerNotes, sequencerSettings, sequencerDimensionsCalculator)
        {
            noteStateSelectedCommand = new UpdateNoteStateCommand(sequencerNotes, NoteState.Selected);
            noteStateUnselectedCommand = new UpdateNoteStateCommand(sequencerNotes, NoteState.Unselected);
        }

        protected override bool CanExecute()
        {
            return Mouse.LeftButton == MouseButtonState.Pressed;
        }

        protected override void DoExecute(Point mousePoint)
        {
            VisualNote actionableNote = sequencerDimensionsCalculator.FindNoteFromPoint(sequencerNotes, mousePoint);
            if (actionableNote != null)
            {
                switch (actionableNote.NoteState)
                {
                    case NoteState.Selected:
                        noteStateUnselectedCommand.Execute(actionableNote.Yield());
                        break;
                    case NoteState.Unselected:
                        noteStateSelectedCommand.Execute(actionableNote.Yield());
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }
    }
}