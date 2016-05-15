using System.Windows;
using JetBrains.Annotations;

namespace Sequencer.Command.MousePointCommand
{
    public sealed class DeleteNoteFromPointCommand : MousePointNoteCommand
    {
        private readonly DeleteNotesCommand deleteNotesCommand;

        public DeleteNoteFromPointCommand([NotNull] SequencerNotes sequencerNotes,
            [NotNull] SequencerSettings sequencerSettings, [NotNull] SequencerDimensionsCalculator sequencerDimensionsCalculator)
            : base(sequencerNotes, sequencerSettings, sequencerDimensionsCalculator)
        {
            deleteNotesCommand = new DeleteNotesCommand(sequencerNotes);
        }

        protected override bool CanExecute()
        {
            return true;
        }

        protected override void DoExecute(Point mousePoint)
        {
            VisualNote noteToDelete = SequencerDimensionsCalculator.FindNoteFromPoint(SequencerNotes, mousePoint);
            deleteNotesCommand.Execute(noteToDelete.Yield());
        }
    }
}