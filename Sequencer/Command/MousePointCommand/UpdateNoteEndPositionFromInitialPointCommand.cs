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
    public sealed class UpdateNoteEndPositionFromInitialPointCommand : MousePointNoteCommand
    {
        private Position initialEndPosition;
        private int beatsDelta;

        public UpdateNoteEndPositionFromInitialPointCommand(IMousePoint initialMousePoint, [NotNull] ISequencerNotes sequencerNotes,
            [NotNull] SequencerSettings sequencerSettings, [NotNull] ISequencerDimensionsCalculator sequencerDimensionsCalculator)
            : base(sequencerNotes, sequencerSettings, sequencerDimensionsCalculator)
        {
            initialEndPosition = SequencerDimensionsCalculator.FindPositionFromPoint(initialMousePoint);
        }

        protected override bool CanExecute => MouseOperator.CanModifyNote;

        protected override void DoExecute(IMousePoint mousePoint)
        {
            MoveNotePositions(mousePoint);
        }

        private void MoveNotePositions(IMousePoint mousePoint)
        {
            Position newEndPosition = SequencerDimensionsCalculator.FindPositionFromPoint(mousePoint);

            int newBeatsDelta = newEndPosition.SummedBeat(SequencerSettings.TimeSignature) -
                                initialEndPosition.SummedBeat(SequencerSettings.TimeSignature);

            if (newBeatsDelta != beatsDelta)
            {
                beatsDelta = newBeatsDelta;

                initialEndPosition = SequencerDimensionsCalculator.FindPositionFromPoint(mousePoint);

                foreach (VisualNote selectedNote in SequencerNotes.SelectedNotes)
                {
                    selectedNote.EndPosition = selectedNote.EndPosition.PositionRelativeByBeats(beatsDelta, SequencerSettings.TimeSignature);
                }
            }
        }
    }
}