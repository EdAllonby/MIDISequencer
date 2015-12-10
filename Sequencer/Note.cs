using System.Windows.Controls;
using log4net;

namespace Sequencer
{
    public sealed class Note
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(Note));

        private readonly NoteDrawer noteDrawer = new NoteDrawer();
        private readonly int noteValue;
        private readonly Position startPosition;
        private Position endPosition;

        public Note(int id, Position startPosition, Position endPosition, int noteValue)
        {
            Id = id;
            this.startPosition = startPosition;
            this.endPosition = endPosition;
            this.noteValue = noteValue;
        }

        public int Id { get; }

        public void DrawNote(TimeSignature timeSignature, double noteHeight, double beatWidth, Canvas sequencer)
        {
            Log.InfoFormat("Drawing note with start position {0} and end position {1}", startPosition, endPosition);   
            noteDrawer.DrawNote(timeSignature, startPosition, endPosition, noteHeight, beatWidth, sequencer, noteHeight*noteValue - noteHeight);
        }

        public void UpdateNoteLength(TimeSignature timeSignature, Position newEndPosition, double beatWidth)
        {
            Log.InfoFormat("Updating note length with start position {0} to end position {1}", startPosition, endPosition);
            endPosition = newEndPosition;
            noteDrawer.UpdateLength(timeSignature, startPosition, newEndPosition, beatWidth);
        }
    }
}