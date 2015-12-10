using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Sequencer
{
    public sealed class NoteDrawer
    {
        private Rectangle noteRectangle;

        public void DrawNote(TimeSignature timeSignature, Position startPosition, Position endPosition, double noteHeight, double beatWidth, Canvas sequencer, double clampedNote)
        {
            double noteStartLocation = GetPointFromPosition(timeSignature, startPosition, beatWidth);
            double noteWidth = ActualWidthBetweenPositions(timeSignature, startPosition, endPosition, beatWidth);

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

        private double GetPointFromPosition(TimeSignature timeSignature, Position position, double beatWidth)
        {
            return position.SummedBeat(timeSignature)*beatWidth - beatWidth;
        }

        public void UpdateLength(TimeSignature timeSignature, Position startPosition, Position endPosition, double beatWidth)
        {
            noteRectangle.Width = ActualWidthBetweenPositions(timeSignature, startPosition, endPosition, beatWidth);
        }

        private double ActualWidthBetweenPositions(TimeSignature timeSignature, Position startPosition, Position endPosition, double beatWidth)
        {
            double noteStartingPoint = GetPointFromPosition(timeSignature, startPosition, beatWidth);
            double noteEndingPoint = GetPointFromPosition(timeSignature, endPosition, beatWidth);
            double newNoteWidth = noteEndingPoint - noteStartingPoint;

            return newNoteWidth >= 0 ? newNoteWidth : 0;
        }
    }
}