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

        public UpdateNoteStateFromPointCommand([NotNull] ISequencerNotes sequencerNotes, [NotNull] IMouseOperator mouseOperator, [NotNull] IKeyboardStateProcessor keyboardStateProcessor, [NotNull] SequencerSettings sequencerSettings, [NotNull] ISequencerDimensionsCalculator sequencerDimensionsCalculator)
            : base(sequencerNotes, mouseOperator, sequencerSettings, sequencerDimensionsCalculator)
        {
            noteStateSelectedCommand = new UpdateNoteStateCommand(sequencerNotes, keyboardStateProcessor, NoteState.Selected);
            noteStateUnselectedCommand = new UpdateNoteStateCommand(sequencerNotes, keyboardStateProcessor, NoteState.Unselected);
        }

        protected override bool CanExecute => MouseOperator.CanModifyNote;

        protected override void DoExecute(IMousePoint mousePoint)
        {
            IVisualNote actionableNote = SequencerDimensionsCalculator.FindNoteFromPoint(SequencerNotes, mousePoint);

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