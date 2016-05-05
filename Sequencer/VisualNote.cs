using System.Windows.Controls;
using log4net;

namespace Sequencer
{
    public sealed class VisualNote
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(VisualNote));

        private readonly NoteDrawer noteDrawer = new NoteDrawer();
        private readonly Pitch pitch;
        private readonly Position startPosition;
        private Position endPosition;

        public VisualNote(int id, Position startPosition, Position endPosition, Pitch pitch)
        {
            Id = id;
            this.startPosition = startPosition;
            this.endPosition = endPosition;
            this.pitch = pitch;
        }

        public int Id { get; }

        public void DrawNote(SequencerSettings sequencerSettings, double noteHeight, double beatWidth, Canvas sequencer)
        {
            Log.InfoFormat("Drawing note with start position {0} and end position {1}", startPosition, endPosition);
            noteDrawer.DrawNote(sequencerSettings, startPosition, endPosition, noteHeight, beatWidth, sequencer, pitch);
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