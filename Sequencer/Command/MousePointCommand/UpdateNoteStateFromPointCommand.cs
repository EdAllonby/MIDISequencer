using System.Windows.Input;
using JetBrains.Annotations;
using Sequencer.Command.NotesCommand;
using Sequencer.Drawing;
using Sequencer.Input;
using Sequencer.Utilities;
using Sequencer.View;

namespace Sequencer.Command.MousePointCommand
{
    public sealed class UpdateNoteStateFromPointCommand : MousePointNoteCommand
    {
        [NotNull] private readonly ISequencerNotes sequencerNotes;
        [NotNull] private readonly IMouseOperator mouseOperator;
        [NotNull] private readonly ISequencerDimensionsCalculator sequencerDimensionsCalculator;
        [NotNull] private readonly UpdateNoteStateCommand noteStateSelectedCommand;
        [NotNull] private readonly UpdateNoteStateCommand noteStateUnselectedCommand;

        public UpdateNoteStateFromPointCommand([NotNull] ISequencerNotes sequencerNotes, [NotNull] IMouseOperator mouseOperator,
            [NotNull] IKeyboardStateProcessor keyboardStateProcessor,
            [NotNull] ISequencerDimensionsCalculator sequencerDimensionsCalculator)
        {
            this.sequencerNotes = sequencerNotes;
            this.mouseOperator = mouseOperator;
            this.sequencerDimensionsCalculator = sequencerDimensionsCalculator;
            noteStateSelectedCommand = new UpdateNoteStateCommand(sequencerNotes, keyboardStateProcessor, NoteState.Selected);
            noteStateUnselectedCommand = new UpdateNoteStateCommand(sequencerNotes, keyboardStateProcessor, NoteState.Unselected);
        }

        protected override bool CanExecute => mouseOperator.CanModifyNote;

        protected override void DoExecute(IMousePoint mousePoint)
        {
            IVisualNote actionableNote = sequencerDimensionsCalculator.FindNoteFromPoint(sequencerNotes, mousePoint);

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
                noteStateUnselectedCommand.Execute(sequencerNotes.SelectedNotes);
            }
        }
    }
}