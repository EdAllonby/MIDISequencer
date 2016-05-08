using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using JetBrains.Annotations;

namespace Sequencer.Command
{
    public sealed class DeleteNoteFromPointCommand : MousePointNoteCommand
    {
        private readonly DeleteNotesCommand deleteNotesCommand;

        public DeleteNoteFromPointCommand([NotNull] Canvas sequencerCanvas, [NotNull] List<VisualNote> sequencerNotes, [NotNull] SequencerSettings sequencerSettings,
            [NotNull] SequencerDimensionsCalculator sequencerDimensionsCalculator) : base(sequencerNotes, sequencerSettings, sequencerDimensionsCalculator)
        {
            deleteNotesCommand = new DeleteNotesCommand(sequencerCanvas, sequencerNotes);
        }

        protected override bool CanExecute()
        {
            return true;
        }

        protected override void DoExecute(Point mousePoint)
        {
            VisualNote noteToDelete = sequencerDimensionsCalculator.FindNoteFromPoint(sequencerNotes, mousePoint);
            deleteNotesCommand.Execute(noteToDelete.Yield());
        }
    }
}