using JetBrains.Annotations;
using Sequencer.Domain;
using Sequencer.View.Drawing;
using Sequencer.Visual;
using Sequencer.Visual.Input;

namespace Sequencer.View.Command.MousePointCommand
{
    /// <summary>
    /// Command to update a <see cref="VisualNote" />'s end position based on the current mouse position in the sequencer.
    /// </summary>
    public sealed class UpdateNewlyCreatedNoteCommand : MousePointNoteCommand
    {
        [NotNull] private readonly IMouseOperator mouseOperator;
        [NotNull] private readonly ISequencerDimensionsCalculator sequencerDimensionsCalculator;
        [NotNull] private readonly ISequencerNotes sequencerNotes;
        [NotNull] private readonly TimeSignature timeSignature;

        public UpdateNewlyCreatedNoteCommand([NotNull] ISequencerNotes sequencerNotes, [NotNull] IMouseOperator mouseOperator,
            [NotNull] TimeSignature timeSignature, [NotNull] ISequencerDimensionsCalculator sequencerDimensionsCalculator)
        {
            this.sequencerNotes = sequencerNotes;
            this.mouseOperator = mouseOperator;
            this.timeSignature = timeSignature;
            this.sequencerDimensionsCalculator = sequencerDimensionsCalculator;
        }

        protected override bool CanExecute => mouseOperator.CanModifyNote;

        protected override void DoExecute(IMousePoint mousePosition)
        {
            IPosition currentEndPosition = sequencerDimensionsCalculator.FindPositionFromPoint(mousePosition);
            IPosition nextPosition = currentEndPosition.NextPosition(timeSignature);

            foreach (IVisualNote selectedNote in sequencerNotes.SelectedNotes)
            {
                selectedNote.EndPosition = nextPosition;
            }
        }
    }
}