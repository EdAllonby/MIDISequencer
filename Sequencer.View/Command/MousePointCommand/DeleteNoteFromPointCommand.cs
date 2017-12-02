using JetBrains.Annotations;
using Sequencer.Utilities;
using Sequencer.View.Command.NotesCommand;
using Sequencer.View.Drawing;
using Sequencer.View.Input;
using Sequencer.View.Control;

namespace Sequencer.View.Command.MousePointCommand
{
    public sealed class DeleteNoteFromPointCommand : MousePointNoteCommand
    {
        [NotNull] private readonly ISequencerNotes sequencerNotes;
        [NotNull] private readonly ISequencerDimensionsCalculator sequencerDimensionsCalculator;
        [NotNull] private readonly IDeleteNotesCommand deleteNotesCommand;

        public DeleteNoteFromPointCommand([NotNull] ISequencerNotes sequencerNotes,
            [NotNull] ISequencerDimensionsCalculator sequencerDimensionsCalculator, [NotNull] IDeleteNotesCommand deleteNotesCommand)
        {
            this.sequencerNotes = sequencerNotes;
            this.sequencerDimensionsCalculator = sequencerDimensionsCalculator;
            this.deleteNotesCommand = deleteNotesCommand;
        }

        protected override bool CanExecute => true;

        protected override void DoExecute(IMousePoint mousePoint)
        {
            IVisualNote noteToDelete = sequencerDimensionsCalculator.FindNoteFromPoint(sequencerNotes, mousePoint);

            if (noteToDelete != null)
            {
                deleteNotesCommand.Execute(noteToDelete.Yield());
            }
        }
    }
}