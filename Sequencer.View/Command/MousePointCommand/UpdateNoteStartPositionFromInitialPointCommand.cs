using JetBrains.Annotations;
using Sequencer.Domain;
using Sequencer.Shared;
using Sequencer.View.Control;
using Sequencer.View.Drawing;
using Sequencer.View.Input;

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
        private int beatsDelta;
        [NotNull] private IPosition initialStartPosition;

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

            int newBeatsDelta = newStartPosition.SummedBeat(sequencerSettings.TimeSignature) -
                                initialStartPosition.SummedBeat(sequencerSettings.TimeSignature);

            if (newBeatsDelta != beatsDelta)
            {
                beatsDelta = newBeatsDelta;

                initialStartPosition = sequencerDimensionsCalculator.FindPositionFromPoint(mousePoint);

                foreach (IVisualNote selectedNote in sequencerNotes.SelectedNotes)
                {
                    selectedNote.StartPosition = selectedNote.StartPosition.PositionRelativeByBeats(beatsDelta, sequencerSettings.TimeSignature);
                }
            }
        }

        protected override void DoExecute(IMousePoint mousePoint)
        {
            MoveNotePositions(mousePoint);
        }
    }
}