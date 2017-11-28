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
    public sealed class UpdateNoteStartPositionFromInitialPointCommand : MousePointNoteCommand
    {
        private Position initialStartPosition;
        private int beatsDelta;

        public UpdateNoteStartPositionFromInitialPointCommand(IMousePoint initialMousePoint, [NotNull] ISequencerNotes sequencerNotes,
            [NotNull] SequencerSettings sequencerSettings, [NotNull] ISequencerDimensionsCalculator sequencerDimensionsCalculator)
            : base(sequencerNotes, sequencerSettings, sequencerDimensionsCalculator)
        {
            initialStartPosition = SequencerDimensionsCalculator.FindPositionFromPoint(initialMousePoint);
        }

        protected override bool CanExecute => MouseOperator.CanModifyNote;

        protected override void DoExecute(IMousePoint mousePoint)
        {
            MoveNotePositions(mousePoint);
        }

        private void MoveNotePositions(IMousePoint mousePoint)
        {
            Position newStartPosition = SequencerDimensionsCalculator.FindPositionFromPoint(mousePoint);

            int newBeatsDelta = newStartPosition.SummedBeat(SequencerSettings.TimeSignature) -
                                initialStartPosition.SummedBeat(SequencerSettings.TimeSignature);

            if (newBeatsDelta != beatsDelta)
            {
                beatsDelta = newBeatsDelta;

                initialStartPosition = SequencerDimensionsCalculator.FindPositionFromPoint(mousePoint);

                foreach (VisualNote selectedNote in SequencerNotes.SelectedNotes)
                {
                    selectedNote.StartPosition = selectedNote.StartPosition.PositionRelativeByBeats(beatsDelta, SequencerSettings.TimeSignature);
                }
            }
        }
    }
}