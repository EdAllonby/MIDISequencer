using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using log4net;
using Sequencer.Domain;

namespace Sequencer.Drawing
{
    public sealed class NoteDrawer
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(NoteDrawer));

        private readonly Rectangle noteRectangle;
        private readonly Rectangle velocityRectangle;
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

            noteRectangle = new Rectangle
            {
                Stroke = new SolidColorBrush(sequencerSettings.LineColour),
                StrokeThickness = 0.5
            };

            velocityRectangle = new Rectangle
            {
                Fill = new SolidColorBrush(sequencerSettings.LineColour)
            };

            sequencer.Children.Add(noteRectangle);
            sequencer.Children.Add(velocityRectangle);

            // We always want the notes to be in the foreground.
            Panel.SetZIndex(noteRectangle, 99);
            Panel.SetZIndex(velocityRectangle, 100);
        }

        public void DrawNote(Pitch pitch, Velocity velocity, Position startPosition, Position endPosition, NoteState noteState)
        {
            Log.InfoFormat("Drawing note length with start position {0} to end position {1}", startPosition, endPosition);

            double beatWidth = sequencerDimensionsCalculator.BeatWidth;
            double noteHeight = sequencerDimensionsCalculator.NoteHeight;

            double noteWidth = ActualWidthBetweenPositions(startPosition, endPosition, beatWidth);
            double noteStartHeight = GetPointFromPitch(pitch, sequencer.ActualHeight, noteHeight);

            noteRectangle.Height = noteHeight;
            noteRectangle.Width = noteWidth;

            SetNoteColour(noteState);

            double noteStartLocation = GetPointFromPosition(startPosition, sequencerDimensionsCalculator.BeatWidth);

            SetRectanglePosition(noteRectangle, noteStartLocation, noteStartHeight);

            double velocityHeight = noteHeight * 0.3;
            double velocityStartHeight = (noteStartHeight + (noteHeight/2)) - (velocityHeight/2);

            velocityRectangle.Height = velocityHeight;
            velocityRectangle.Width = noteWidth * velocity.Volume;

            SetRectanglePosition(velocityRectangle, noteStartLocation, velocityStartHeight);
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
        
        public bool IntersectsWith(Rect rectangle)
        {
            var noteRect = new Rect(Canvas.GetLeft(noteRectangle), Canvas.GetTop(noteRectangle), noteRectangle.Width, noteRectangle.Height);

            return rectangle.IntersectsWith(noteRect);
        }

        public void RemoveNote()
        {
            sequencer.Children.Remove(noteRectangle);
            sequencer.Children.Remove(velocityRectangle);
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

        private static void SetRectanglePosition(Rectangle rectangle, double leftPosition, double topPosition)
        {
            Canvas.SetLeft(rectangle, leftPosition);
            Canvas.SetTop(rectangle, topPosition);
        }
    }
}