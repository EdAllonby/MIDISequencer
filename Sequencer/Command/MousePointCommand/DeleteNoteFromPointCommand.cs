using System.Windows;
using JetBrains.Annotations;
using Sequencer.Command.NotesCommand;
using Sequencer.Domain;
using Sequencer.Drawing;
using Sequencer.Utilities;
using Sequencer.View;

namespace Sequencer.Command.MousePointCommand
{
    public sealed class DeleteNoteFromPointCommand : MousePointNoteCommand
    {
        private readonly DeleteNotesCommand deleteNotesCommand;

        public DeleteNoteFromPointCommand([NotNull] ISequencerNotes sequencerNotes,
            [NotNull] SequencerSettings sequencerSettings, [NotNull] ISequencerDimensionsCalculator sequencerDimensionsCalculator)
            : base(sequencerNotes, sequencerSettings, sequencerDimensionsCalculator)
        {
            deleteNotesCommand = new DeleteNotesCommand(sequencerNotes);
        }

        protected override bool CanExecute => true;

        protected override void DoExecute(IMousePoint mousePoint)
        {
            IVisualNote noteToDelete = SequencerDimensionsCalculator.FindNoteFromPoint(SequencerNotes, mousePoint);
            deleteNotesCommand.Execute(noteToDelete.Yield());
        }

    }
}