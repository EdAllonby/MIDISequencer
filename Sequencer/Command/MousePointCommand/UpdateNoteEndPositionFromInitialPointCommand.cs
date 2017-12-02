using JetBrains.Annotations;
using Sequencer.Domain;
using Sequencer.Drawing;
using Sequencer.Input;
using Sequencer.Shared;
using Sequencer.View;

namespace Sequencer.Command.MousePointCommand
{
    /// <summary>
    /// Command to update a <see cref="VisualNote" />'s end position based on the current mouse position in the sequencer.
    /// </summary>
    public sealed class UpdateNoteEndPositionFromInitialPointCommand : MousePointNoteCommand
    {
        [NotNull] private readonly IMouseOperator mouseOperator;
        [NotNull] private readonly ISequencerNotes sequencerNotes;
        [NotNull] private readonly SequencerSettings sequencerSettings;
        [NotNull] private readonly ISequencerDimensionsCalculator sequencerDimensionsCalculator;
        [NotNull] private IPosition initialEndPosition;
        private int beatsDelta;

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

        protected override void DoExecute(IMousePoint mousePoint)
        {
            MoveNotePositions(mousePoint);
        }

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
    }
}