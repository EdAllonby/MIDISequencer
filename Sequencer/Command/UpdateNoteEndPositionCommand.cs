using System.Windows;

namespace Sequencer.Command
{
    /// <summary>
    /// Command to update a <see cref="VisualNote" />'s end position based on the current mouse position in the sequencer.
    /// </summary>
    public sealed class UpdateNoteEndPositionCommand
    {
        private readonly SequencerSettings sequencerSettings;
        private readonly SequencerDimensionsCalculator sequencerDimensionsCalculator;

        public UpdateNoteEndPositionCommand(SequencerSettings sequencerSettings, SequencerDimensionsCalculator sequencerDimensionsCalculator)
        {
            this.sequencerSettings = sequencerSettings;
            this.sequencerDimensionsCalculator = sequencerDimensionsCalculator;
        }

        public void UpdateNoteEndPosition(VisualNote noteToUpdate, Point mousePosition)
        {
            Position currentEndPosition = sequencerDimensionsCalculator.FindNotePositionFromPoint(mousePosition);
            Position nextPosition = currentEndPosition.NextPosition(sequencerSettings.TimeSignature);
            noteToUpdate.UpdateNoteLength(sequencerSettings.TimeSignature, nextPosition, sequencerDimensionsCalculator.BeatWidth);
        }
    }
}