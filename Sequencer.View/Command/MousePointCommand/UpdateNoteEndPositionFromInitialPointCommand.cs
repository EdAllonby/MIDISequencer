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
        private int beatsDelta;
        [NotNull] private IPosition initialEndPosition;

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

            int newBeatsDelta = newEndPosition.SummedBeat(sequencerSettings.TimeSignature) -
                                initialEndPosition.SummedBeat(sequencerSettings.TimeSignature);

            if (newBeatsDelta != beatsDelta)
            {
                beatsDelta = newBeatsDelta;

                initialEndPosition = sequencerDimensionsCalculator.FindPositionFromPoint(mousePoint);

                foreach (IVisualNote selectedNote in sequencerNotes.SelectedNotes)
                {
                    selectedNote.EndPosition = selectedNote.EndPosition.PositionRelativeByBeats(beatsDelta, sequencerSettings.TimeSignature);
                }
            }
        }

        protected override void DoExecute(IMousePoint mousePoint)
        {
            MoveNotePositions(mousePoint);
        }
    }
}