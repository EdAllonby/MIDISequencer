using System.Windows.Controls;

namespace Sequencer
{
    public sealed class Note
    {
        private readonly NoteDrawer noteDrawer = new NoteDrawer();
        private readonly int noteValue;
        private readonly Position startPosition;
        private Position endPosition;

        public Note(int id, Position startPosition, int noteValue)
        {
            Id = id;
            this.startPosition = startPosition;
            this.noteValue = noteValue;
        }

        public int Id { get; }

        public void DrawNote(TimeSignature timeSignature, double noteHeight, double beatWidth, Canvas sequencer)
        {
            noteDrawer.DrawNote(timeSignature, startPosition, endPosition, noteHeight, beatWidth, sequencer, noteHeight*noteValue - noteHeight);
        }

        public void UpdateNoteLength(TimeSignature timeSignature, Position newEndPosition, double beatWidth)
        {
            endPosition = newEndPosition;
            noteDrawer.UpdateLength(timeSignature, startPosition, newEndPosition, beatWidth);
        }
    }
}