using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using Sequencer.Domain;

namespace Sequencer.Drawing
{
    public sealed class NoteDrawer
    {
        private readonly Rectangle noteRectangle;
        private readonly IDigitalAudioProtocol protocol;
        private readonly Canvas sequencer;
        private readonly SequencerDimensionsCalculator sequencerDimensionsCalculator;
        private readonly SequencerSettings sequencerSettings;
        private readonly TimeSignature timeSignature;

        public NoteDrawer(Canvas sequencer, SequencerSettings sequencerSettings, SequencerDimensionsCalculator sequencerDimensionsCalculator)
        {
            this.sequencer = sequencer;
            this.sequencerSettings = sequencerSettings;
            this.sequencerDimensionsCalculator = sequencerDimensionsCalculator;
            timeSignature = sequencerSettings.TimeSignature;
            protocol = sequencerSettings.Protocol;

            noteRectangle = new Rectangle
            {
                Stroke = new SolidColorBrush(sequencerSettings.LineColour),
                StrokeThickness = 0.5
            };

            sequencer.Children.Add(noteRectangle);

            // We always want the notes to be in the foreground.
            Panel.SetZIndex(noteRectangle, 99);
        }

        public void DrawNote(Pitch pitch, Position startPosition, Position endPosition, NoteState noteState)
        {
            double beatWidth = sequencerDimensionsCalculator.BeatWidth;
            double noteHeight = sequencerDimensionsCalculator.NoteHeight;

            double noteWidth = ActualWidthBetweenPositions(startPosition, endPosition, beatWidth);
            double noteStartHeight = GetPointFromPitch(pitch, sequencer.ActualHeight, noteHeight);

            noteRectangle.Height = noteHeight;
            noteRectangle.Width = noteWidth;

            SetNoteColour(noteState);

            double noteStartLocation = GetPointFromPosition(startPosition, sequencerDimensionsCalculator.BeatWidth);
            Canvas.SetLeft(noteRectangle, noteStartLocation);
            Canvas.SetTop(noteRectangle, noteStartHeight);
        }

        public void SetNoteColour(NoteState noteState)
        {
            switch (noteState)
            {
                case NoteState.Selected:
                    noteRectangle.Fill = new SolidColorBrush(sequencerSettings.SelectedNoteColour);
                    break;
                case NoteState.Unselected:
                    noteRectangle.Fill = new SolidColorBrush(sequencerSettings.UnselectedNoteColour);
                    break;
            }
        }

        public void RemoveNote()
        {
            sequencer.Children.Remove(noteRectangle);
        }

        private double GetPointFromPitch(Pitch pitch, double sequencerHeight, double noteHeight)
        {
            int halfStepDifference = PitchStepCalculator.FindStepsFromPitches(sequencerSettings.lowestPitch, pitch);
            double relativePitchPosition = (noteHeight*halfStepDifference) + noteHeight;
            return sequencerHeight - relativePitchPosition;
        }

        private double ActualWidthBetweenPositions(Position startPosition, Position endPosition, double beatWidth)
        {
            double noteStartingPoint = GetPointFromPosition(startPosition, beatWidth);
            double noteEndingPoint = GetPointFromPosition(endPosition, beatWidth);
            double newNoteWidth = noteEndingPoint - noteStartingPoint;

            return newNoteWidth >= 0 ? newNoteWidth : 0;
        }

        private double GetPointFromPosition(Position position, double beatWidth)
        {
            return (position.SummedBeat(timeSignature)*beatWidth) - beatWidth;
        }

        public bool IntersectsWith(Rect rectangle)
        {
            var noteRect = new Rect(Canvas.GetLeft(noteRectangle), Canvas.GetTop(noteRectangle), noteRectangle.Width, noteRectangle.Height);

            return rectangle.IntersectsWith(noteRect);
        }
    }
}