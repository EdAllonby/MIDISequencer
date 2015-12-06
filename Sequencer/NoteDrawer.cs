using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Sequencer
{
    public sealed class NoteDrawer
    {
        private Rectangle noteRectangle;

        public void DrawNote(Position startPosition, Position endPosition, double noteHeight, double beatWidth, Canvas sequencer, double clampedNote)
        {
            double noteStartLocation = GetPointFromPosition(startPosition, beatWidth);

            double noteWidth = 0;

            if (endPosition != null)
            {
                noteWidth = ActualWidthBetweenPositions(startPosition, endPosition, beatWidth);
            }

            noteRectangle = new Rectangle
            {
                Fill = new SolidColorBrush(Colors.DarkRed),
                Height = noteHeight,
                Width = noteWidth,
                Stroke = new SolidColorBrush(Colors.Black),
                StrokeThickness = 0.5
            };

            sequencer.Children.Add(noteRectangle);
            Canvas.SetLeft(noteRectangle, noteStartLocation);
            Canvas.SetTop(noteRectangle, clampedNote);
        }

        private double GetPointFromPosition(Position position, double beatWidth)
        {
            return position.SummedBeat(4, 4)*beatWidth - beatWidth;
        }


        public void UpdateLength(Position startPosition, Position endPosition, double beatWidth)
        {
            noteRectangle.Width = ActualWidthBetweenPositions(startPosition, endPosition, beatWidth);
        }

        private double ActualWidthBetweenPositions(Position startPosition, Position endPosition, double beatWidth)
        {
            double noteStartingPoint = GetPointFromPosition(startPosition, beatWidth);
            double noteEndingPoint = GetPointFromPosition(endPosition, beatWidth);
            double newNoteWidth = noteEndingPoint - noteStartingPoint;

            return newNoteWidth >= 0 ? newNoteWidth : 0;
        }
    }
}