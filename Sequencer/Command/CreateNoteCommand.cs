using System.Windows;

namespace Sequencer.Command
{
    /// <summary>
    /// Creates a <see cref="VisualNote" /> with correct pitch relative to the sequencer.
    /// </summary>
    public class CreateNoteCommand
    {
        private readonly SequencerSettings sequencerSettings;
        private readonly SequencerDimensionsCalculator sequencerDimensionsCalculator;

        public CreateNoteCommand(SequencerSettings sequencerSettings, SequencerDimensionsCalculator sequencerDimensionsCalculator)
        {
            this.sequencerSettings = sequencerSettings;
            this.sequencerDimensionsCalculator = sequencerDimensionsCalculator;
        }

        public VisualNote CreateNote(Point mousePoint)
        {
            Position notePosition = sequencerDimensionsCalculator.FindNotePositionFromPoint(mousePoint);
            Pitch pitch = sequencerDimensionsCalculator.FindPitch(mousePoint);

            Position defaultEndPosition = GetDefaultEndPosition(notePosition);

            return new VisualNote(notePosition, defaultEndPosition, pitch);
        }

        private Position GetDefaultEndPosition(Position notePosition)
        {
            return notePosition.NextPosition(sequencerSettings.TimeSignature);
        }
    }
}