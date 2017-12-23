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
    public sealed class UpdateNoteStartPositionFromInitialPointCommand : MousePointNoteCommand
    {
        [NotNull] private readonly IMouseOperator mouseOperator;
        [NotNull] private readonly ISequencerDimensionsCalculator sequencerDimensionsCalculator;
        [NotNull] private readonly ISequencerNotes sequencerNotes;
        [NotNull] private readonly SequencerSettings sequencerSettings;
        [NotNull] private IPosition initialStartPosition;
        private int ticksDelta;

        public UpdateNoteStartPositionFromInitialPointCommand([NotNull] IMousePoint initialMousePoint, [NotNull] IMouseOperator mouseOperator,
            [NotNull] ISequencerNotes sequencerNotes, [NotNull] SequencerSettings sequencerSettings,
            [NotNull] ISequencerDimensionsCalculator sequencerDimensionsCalculator)
        {
            this.mouseOperator = mouseOperator;
            this.sequencerNotes = sequencerNotes;
            this.sequencerSettings = sequencerSettings;
            this.sequencerDimensionsCalculator = sequencerDimensionsCalculator;
            initialStartPosition = sequencerDimensionsCalculator.FindPositionFromPoint(initialMousePoint);
        }

        protected override bool CanExecute => mouseOperator.CanModifyNote;

        private void MoveNotePositions([NotNull] IMousePoint mousePoint)
        {
            IPosition newStartPosition = sequencerDimensionsCalculator.FindPositionFromPoint(mousePoint);

            int newTicksDelta = newStartPosition.TotalTicks(sequencerSettings.TimeSignature, sequencerSettings.TicksPerQuarterNote) -
                                initialStartPosition.TotalTicks(sequencerSettings.TimeSignature, sequencerSettings.TicksPerQuarterNote);

            if (newTicksDelta != ticksDelta)
            {
                ticksDelta = newTicksDelta;

                initialStartPosition = sequencerDimensionsCalculator.FindPositionFromPoint(mousePoint);

                foreach (IVisualNote selectedNote in sequencerNotes.SelectedNotes)
                {
                    selectedNote.StartPosition = selectedNote.StartPosition.PositionRelativeByTicks(ticksDelta, sequencerSettings.TimeSignature, sequencerSettings.TicksPerQuarterNote);
                }
            }
        }

        protected override void DoExecute(IMousePoint mousePoint)
        {
            MoveNotePositions(mousePoint);
        }
    }
}