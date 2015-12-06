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

        public void DrawNote(double noteHeight, double beatWidth, Canvas sequencer)
        {
            noteDrawer.DrawNote(startPosition, endPosition, noteHeight, beatWidth, sequencer, noteHeight*noteValue - noteHeight);
        }

        public void UpdateNoteLength(Position newEndPosition, double beatWidth)
        {
            endPosition = newEndPosition;
            noteDrawer.UpdateLength(startPosition, newEndPosition, beatWidth);
        }
    }
}