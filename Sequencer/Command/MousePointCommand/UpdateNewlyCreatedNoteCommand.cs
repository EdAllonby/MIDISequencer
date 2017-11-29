using System.Windows;
using JetBrains.Annotations;
using Sequencer.Domain;
using Sequencer.Drawing;
using Sequencer.Input;
using Sequencer.View;

namespace Sequencer.Command.MousePointCommand
{
    /// <summary>
    /// Command to update a <see cref="VisualNote" />'s end position based on the current mouse position in the sequencer.
    /// </summary>
    public sealed class UpdateNewlyCreatedNoteCommand : MousePointNoteCommand
    {
        public UpdateNewlyCreatedNoteCommand([NotNull] ISequencerNotes sequencerNotes, [NotNull] IMouseOperator mouseOperator,
            [NotNull] SequencerSettings sequencerSettings, [NotNull] ISequencerDimensionsCalculator sequencerDimensionsCalculator)
            : base(sequencerNotes, mouseOperator, sequencerSettings, sequencerDimensionsCalculator)
        {
        }

        protected override bool CanExecute => MouseOperator.CanModifyNote;

        protected override void DoExecute(IMousePoint mousePosition)
        {
            Position currentEndPosition = SequencerDimensionsCalculator.FindPositionFromPoint(mousePosition);
            Position nextPosition = currentEndPosition.NextPosition(SequencerSettings.TimeSignature);

            foreach (VisualNote selectedNote in SequencerNotes.SelectedNotes)
            {
                selectedNote.EndPosition = nextPosition;
            }
        }
    }
}