using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using JetBrains.Annotations;

namespace Sequencer.Command
{
    public sealed class DeleteNoteFromPointCommand : MousePointNoteCommand
    {
        private readonly DeleteNotesCommand deleteNotesCommand;
        private readonly Canvas sequencerCanvas;

        public DeleteNoteFromPointCommand([NotNull] Canvas sequencerCanvas, [NotNull] List<VisualNote> sequencerNotes, [NotNull] SequencerSettings sequencerSettings,
            [NotNull] SequencerDimensionsCalculator sequencerDimensionsCalculator) : base(sequencerNotes, sequencerSettings, sequencerDimensionsCalculator)
        {
            this.sequencerCanvas = sequencerCanvas;
            deleteNotesCommand = new DeleteNotesCommand(sequencerCanvas, sequencerNotes);
        }

        public override void Execute(Point mousePoint)
        {
            VisualNote noteToDelete = sequencerDimensionsCalculator.FindNoteFromPoint(sequencerNotes, mousePoint);
            deleteNotesCommand.Execute(noteToDelete.Yield());
        }
    }
}