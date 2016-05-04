using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Sequencer
{
    public sealed class NoteDrawer
    {
        private Rectangle noteRectangle;

        public void DrawNote(SequencerSettings sequencerSettings, Position startPosition, Position endPosition, double noteHeight, double beatWidth, Canvas sequencer, Pitch pitch)
        {
            TimeSignature timeSignature = sequencerSettings.TimeSignature;

            double noteStartLocation = GetPointFromPosition(timeSignature, startPosition, beatWidth);
            double noteWidth = ActualWidthBetweenPositions(timeSignature, startPosition, endPosition, beatWidth);
            double noteStartHeight = GetPointFromPitch(pitch, sequencer.ActualHeight, noteHeight, sequencerSettings.LowestPitch);
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
            Canvas.SetTop(noteRectangle, noteStartHeight);
        }

        private static double GetPointFromPitch(Pitch pitch, double sequencerHeight, double noteHeight, Pitch startingPitch)
        {
            int pitchDelta = pitch.MidiNoteNumber - startingPitch.MidiNoteNumber;
            double relativePitchPosition = noteHeight*pitchDelta + noteHeight;
            return sequencerHeight - relativePitchPosition;
        }

        public void UpdateLength(TimeSignature timeSignature, Position startPosition, Position endPosition, double beatWidth)
        {
            noteRectangle.Width = ActualWidthBetweenPositions(timeSignature, startPosition, endPosition, beatWidth);
        }

        private static double ActualWidthBetweenPositions(TimeSignature timeSignature, Position startPosition, Position endPosition, double beatWidth)
        {
            double noteStartingPoint = GetPointFromPosition(timeSignature, startPosition, beatWidth);
            double noteEndingPoint = GetPointFromPosition(timeSignature, endPosition, beatWidth);
            double newNoteWidth = noteEndingPoint - noteStartingPoint;

            return newNoteWidth >= 0 ? newNoteWidth : 0;
        }

        private static double GetPointFromPosition(TimeSignature timeSignature, Position position, double beatWidth)
        {
            return position.SummedBeat(timeSignature)*beatWidth - beatWidth;
        }
    }
}