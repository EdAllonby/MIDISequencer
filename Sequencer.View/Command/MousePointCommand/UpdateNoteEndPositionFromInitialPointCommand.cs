using JetBrains.Annotations;
using Sequencer.Domain;
using Sequencer.Domain.Settings;
using Sequencer.Visual;
using Sequencer.Visual.Input;

namespace Sequencer.View.Command.MousePointCommand
{
    /// <summary>
    /// Command to update a <see cref="VisualNote" />'s end position based on the current mouse position in the sequencer.
    /// </summary>
    public sealed class UpdateNoteEndPositionFromInitialPointCommand : MousePointNoteCommand
    {
        [NotNull] private readonly IMouseOperator mouseOperator;
        [NotNull] private readonly ISequencerDimensionsCalculator sequencerDimensionsCalculator;
        [NotNull] private readonly ISequencerNotes sequencerNotes;
        [NotNull] private readonly SequencerSettings sequencerSettings;
        [NotNull] private IPosition initialEndPosition;
        private int ticksDelta;

        public UpdateNoteEndPositionFromInitialPointCommand([NotNull] IMousePoint initialMousePoint,
            [NotNull] IMouseOperator mouseOperator, [NotNull] ISequencerNotes sequencerNotes,
            [NotNull] SequencerSettings sequencerSettings, [NotNull] ISequencerDimensionsCalculator sequencerDimensionsCalculator)
        {
            this.mouseOperator = mouseOperator;
            this.sequencerNotes = sequencerNotes;
            this.sequencerSettings = sequencerSettings;
            this.sequencerDimensionsCalculator = sequencerDimensionsCalculator;
            initialEndPosition = sequencerDimensionsCalculator.FindPositionFromPoint(initialMousePoint);
        }

        protected override bool CanExecute => mouseOperator.CanModifyNote;

        private void MoveNotePositions([NotNull] IMousePoint mousePoint)
        {
            IPosition newEndPosition = sequencerDimensionsCalculator.FindPositionFromPoint(mousePoint);

            int newTicksDelta = newEndPosition.TotalTicks(sequencerSettings.TimeSignature, sequencerSettings.TicksPerQuarterNote) -
                                initialEndPosition.TotalTicks(sequencerSettings.TimeSignature, sequencerSettings.TicksPerQuarterNote);

            // The reference point moves with each step, so any non-zero delta is new movement. (Comparing with
            // the previous delta skipped every other step of the same size.)
            if (newTicksDelta != 0)
            {
                ticksDelta = newTicksDelta;

                initialEndPosition = sequencerDimensionsCalculator.FindPositionFromPoint(mousePoint);

                foreach (IVisualNote selectedNote in sequencerNotes.SelectedNotes)
                {
                    selectedNote.EndPosition = selectedNote.EndPosition.PositionRelativeByTicks(ticksDelta, sequencerSettings.TimeSignature, sequencerSettings.TicksPerQuarterNote);
                }
            }
        }

        protected override void DoExecute(IMousePoint mousePoint)
        {
            MoveNotePositions(mousePoint);
        }
    }
}