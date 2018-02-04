using System.Windows.Input;
using JetBrains.Annotations;
using Sequencer.Utilities;
using Sequencer.View.Command.NotesCommand;
using Sequencer.Visual;
using Sequencer.Visual.Input;

namespace Sequencer.View.Command.MousePointCommand
{
    public sealed class UpdateNoteStateFromPointCommand : MousePointNoteCommand
    {
        [NotNull] private readonly IMouseOperator mouseOperator;
        [NotNull] private readonly IKeyboardStateProcessor keyboardStateProcessor;
        [NotNull] private readonly INotesCommand noteStateSelectedCommand;
        [NotNull] private readonly INotesCommand noteStateUnselectedCommand;
        [NotNull] private readonly ISequencerDimensionsCalculator sequencerDimensionsCalculator;
        [NotNull] private readonly ISequencerNotes sequencerNotes;

        public UpdateNoteStateFromPointCommand([NotNull] ISequencerNotes sequencerNotes, [NotNull] IMouseOperator mouseOperator,
            [NotNull] IKeyboardStateProcessor keyboardStateProcessor, [NotNull] ISequencerDimensionsCalculator sequencerDimensionsCalculator, 
            [NotNull] INoteStateCommandFactory noteStateCommandFactory)
        {
            this.sequencerNotes = sequencerNotes;
            this.mouseOperator = mouseOperator;
            this.keyboardStateProcessor = keyboardStateProcessor;
            this.sequencerDimensionsCalculator = sequencerDimensionsCalculator;
            noteStateSelectedCommand = noteStateCommandFactory.CreateNoteStateCommand(NoteState.Selected);
            noteStateUnselectedCommand = noteStateCommandFactory.CreateNoteStateCommand(NoteState.Unselected);
        }

        protected override bool CanExecute => mouseOperator.CanModifyNote;

        protected override void DoExecute(IMousePoint mousePoint)
        {
            IVisualNote actionableNote = sequencerDimensionsCalculator.FindNoteFromPoint(sequencerNotes, mousePoint);
            
            if (actionableNote != null)
            {
                if (actionableNote.NoteState == NoteState.Selected && keyboardStateProcessor.IsKeyDown(Key.LeftCtrl))
                {
                    noteStateUnselectedCommand.Execute(actionableNote.Yield());
                }
                else if (actionableNote.NoteState == NoteState.Unselected)
                {
                    noteStateSelectedCommand.Execute(actionableNote.Yield());
                }
            }
            else if (!keyboardStateProcessor.IsKeyDown(Key.LeftCtrl))
            {
                noteStateUnselectedCommand.Execute(sequencerNotes.SelectedNotes);
            }
        }
    }
}