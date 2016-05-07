using System.Windows.Controls;
using JetBrains.Annotations;
using log4net;

namespace Sequencer
{
    public sealed class VisualNote
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(VisualNote));

        [NotNull] private readonly NoteDrawer noteDrawer = new NoteDrawer();

        private readonly Pitch pitch;
        private readonly Position startPosition;
        private Position endPosition;

        public VisualNote([NotNull] Position startPosition, [NotNull] Position endPosition, [NotNull] Pitch pitch)
        {
            this.startPosition = startPosition;
            this.endPosition = endPosition;
            this.pitch = pitch;
        }

        public void Draw(SequencerDimensionsCalculator sequencerDimensionsCalculator , SequencerSettings sequencerSettings, Canvas sequencer)
        {
            Log.InfoFormat("Drawing note with start position {0} and end position {1}", startPosition, endPosition);
            noteDrawer.DrawNote(sequencerSettings, startPosition, endPosition, sequencerDimensionsCalculator.NoteHeight, sequencerDimensionsCalculator.BeatWidth, sequencer, pitch);
        }

        public void UpdateNoteLength(TimeSignature timeSignature, Position newEndPosition, double beatWidth)
        {
            if (newEndPosition >= startPosition)
            {
                Log.InfoFormat("Updating note length with start position {0} to end position {1}", startPosition, endPosition);
                endPosition = newEndPosition;
                noteDrawer.UpdateLength(timeSignature, startPosition, newEndPosition, beatWidth);
            }
        }
    }
}