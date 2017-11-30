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
        [NotNull] private readonly ISequencerNotes sequencerNotes;
        [NotNull] private readonly IMouseOperator mouseOperator;
        [NotNull] private readonly SequencerSettings sequencerSettings;
        [NotNull] private readonly ISequencerDimensionsCalculator sequencerDimensionsCalculator;

        public UpdateNewlyCreatedNoteCommand([NotNull] ISequencerNotes sequencerNotes, [NotNull] IMouseOperator mouseOperator,
            [NotNull] SequencerSettings sequencerSettings, [NotNull] ISequencerDimensionsCalculator sequencerDimensionsCalculator)
        {
            this.sequencerNotes = sequencerNotes;
            this.mouseOperator = mouseOperator;
            this.sequencerSettings = sequencerSettings;
            this.sequencerDimensionsCalculator = sequencerDimensionsCalculator;
        }

        protected override bool CanExecute => mouseOperator.CanModifyNote;

        protected override void DoExecute(IMousePoint mousePosition)
        {
            Position currentEndPosition = sequencerDimensionsCalculator.FindPositionFromPoint(mousePosition);
            Position nextPosition = currentEndPosition.NextPosition(sequencerSettings.TimeSignature);

            foreach (VisualNote selectedNote in sequencerNotes.SelectedNotes)
            {
                selectedNote.EndPosition = nextPosition;
            }
        }
    }
}