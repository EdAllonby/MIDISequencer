using System.Windows;
using System.Windows.Input;
using JetBrains.Annotations;
using Sequencer.Command.NotesCommand;
using Sequencer.Domain;
using Sequencer.Drawing;
using Sequencer.Input;
using Sequencer.Utilities;
using Sequencer.View;

namespace Sequencer.Command.MousePointCommand
{
    public sealed class UpdateNoteStateFromPointCommand : MousePointNoteCommand
    {
        private readonly UpdateNoteStateCommand noteStateSelectedCommand;
        private readonly UpdateNoteStateCommand noteStateUnselectedCommand;

        public UpdateNoteStateFromPointCommand([NotNull] SequencerNotes sequencerNotes, [NotNull] SequencerSettings sequencerSettings, [NotNull] SequencerDimensionsCalculator sequencerDimensionsCalculator)
            : base(sequencerNotes, sequencerSettings, sequencerDimensionsCalculator)
        {
            noteStateSelectedCommand = new UpdateNoteStateCommand(sequencerNotes, NoteState.Selected);
            noteStateUnselectedCommand = new UpdateNoteStateCommand(sequencerNotes, NoteState.Unselected);
        }

        protected override bool CanExecute => MouseOperator.CanModifyNote;

        protected override void DoExecute(Point mousePoint)
        {
            VisualNote actionableNote = SequencerDimensionsCalculator.FindNoteFromPoint(SequencerNotes, mousePoint);

            if (actionableNote != null)
            {
                if ((actionableNote.NoteState == NoteState.Selected) && Keyboard.IsKeyDown(Key.LeftCtrl))
                {
                    noteStateUnselectedCommand.Execute(actionableNote.Yield());
                }
                else if (actionableNote.NoteState == NoteState.Unselected)
                {
                    noteStateSelectedCommand.Execute(actionableNote.Yield());
                }
            }
            else if (!Keyboard.IsKeyDown(Key.LeftCtrl))
            {
                noteStateUnselectedCommand.Execute(SequencerNotes.SelectedNotes);
            }
        }
    }
}