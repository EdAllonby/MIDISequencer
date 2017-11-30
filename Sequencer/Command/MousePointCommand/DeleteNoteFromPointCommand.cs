using JetBrains.Annotations;
using Sequencer.Command.NotesCommand;
using Sequencer.Drawing;
using Sequencer.Utilities;
using Sequencer.View;

namespace Sequencer.Command.MousePointCommand
{
    public sealed class DeleteNoteFromPointCommand : MousePointNoteCommand
    {
        [NotNull] private readonly ISequencerNotes sequencerNotes;
        [NotNull] private readonly ISequencerDimensionsCalculator sequencerDimensionsCalculator;
        private readonly DeleteNotesCommand deleteNotesCommand;

        public DeleteNoteFromPointCommand([NotNull] ISequencerNotes sequencerNotes,
            [NotNull] ISequencerDimensionsCalculator sequencerDimensionsCalculator)
        {
            this.sequencerNotes = sequencerNotes;
            this.sequencerDimensionsCalculator = sequencerDimensionsCalculator;
            deleteNotesCommand = new DeleteNotesCommand(sequencerNotes);
        }

        protected override bool CanExecute => true;

        protected override void DoExecute(IMousePoint mousePoint)
        {
            IVisualNote noteToDelete = sequencerDimensionsCalculator.FindNoteFromPoint(sequencerNotes, mousePoint);
            deleteNotesCommand.Execute(noteToDelete.Yield());
        }
    }
}