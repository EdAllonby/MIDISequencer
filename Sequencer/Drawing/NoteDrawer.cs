using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using JetBrains.Annotations;
using log4net;
using Sequencer.Domain;
using Sequencer.Shared;
using Sequencer.Utilities;
using Sequencer.View;

namespace Sequencer.Drawing
{
    public sealed class NoteDrawer
    {
        [NotNull] private static readonly ILog Log = LogExtensions.GetLoggerSafe(typeof(NoteDrawer));

        [NotNull] private readonly Rectangle noteRectangle;
        [NotNull] private readonly IPitchAndPositionCalculator pitchAndPositionCalculator;
        [NotNull] private readonly ISequencerCanvasWrapper sequencer;
        [NotNull] private readonly ISequencerDimensionsCalculator sequencerDimensionsCalculator;
        [NotNull] private readonly SequencerSettings sequencerSettings;
        [NotNull] private readonly TimeSignature timeSignature;
        [NotNull] private readonly Rectangle velocityRectangle;

        public NoteDrawer([NotNull] IPitchAndPositionCalculator pitchAndPositionCalculator, [NotNull] ISequencerCanvasWrapper sequencer, [NotNull] SequencerSettings sequencerSettings,
            [NotNull] ISequencerDimensionsCalculator sequencerDimensionsCalculator)
        {
            this.pitchAndPositionCalculator = pitchAndPositionCalculator;
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

            sequencer.AddChild(noteRectangle);
            sequencer.AddChild(velocityRectangle);

            // We always want the notes to be in the foreground.
            Panel.SetZIndex(noteRectangle, 99);
            Panel.SetZIndex(velocityRectangle, 100);
        }

        public void DrawNote([NotNull] Pitch pitch, [NotNull] Velocity velocity, [NotNull] IPosition startPosition, [NotNull] IPosition endPosition, NoteState noteState)
        {
            Log.InfoFormat("Drawing note length with start position {0} to end position {1}", startPosition, endPosition);

            double beatWidth = sequencerDimensionsCalculator.BeatWidth;
            double noteHeight = sequencerDimensionsCalculator.NoteHeight;

            double noteWidth = ActualWidthBetweenPositions(startPosition, endPosition, beatWidth);
            double noteStartHeight = GetPointFromPitch(pitch, sequencer.Height, noteHeight);

            noteRectangle.Height = noteHeight;
            noteRectangle.Width = noteWidth;

            SetNoteColour(noteState);

            double noteStartLocation = GetPointFromPosition(startPosition, sequencerDimensionsCalculator.BeatWidth);

            SetRectanglePosition(noteRectangle, noteStartLocation, noteStartHeight);

            double velocityHeight = noteHeight*0.3;
            double velocityStartHeight = (noteStartHeight + (noteHeight/2)) - (velocityHeight/2);

            velocityRectangle.Height = velocityHeight;
            velocityRectangle.Width = noteWidth*velocity.Volume;

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
            sequencer.RemoveChild(noteRectangle);
            sequencer.RemoveChild(velocityRectangle);
        }

        private double GetPointFromPitch([NotNull] Pitch pitch, double sequencerHeight, double noteHeight)
        {
            int halfStepDifference = pitchAndPositionCalculator.FindStepsFromPitches(sequencerSettings.LowestPitch, pitch);
            double relativePitchPosition = (noteHeight*halfStepDifference) + noteHeight;
            return sequencerHeight - relativePitchPosition;
        }

        private double ActualWidthBetweenPositions([NotNull] IPosition startPosition, [NotNull] IPosition endPosition, double beatWidth)
        {
            double noteStartingPoint = GetPointFromPosition(startPosition, beatWidth);
            double noteEndingPoint = GetPointFromPosition(endPosition, beatWidth);
            double newNoteWidth = noteEndingPoint - noteStartingPoint;

            return newNoteWidth >= 0 ? newNoteWidth : 0;
        }

        private double GetPointFromPosition([NotNull] IPosition position, double beatWidth)
        {
            return (position.SummedBeat(timeSignature)*beatWidth) - beatWidth;
        }

        private static void SetRectanglePosition([NotNull] UIElement rectangle, double leftPosition, double topPosition)
        {
            Canvas.SetLeft(rectangle, leftPosition);
            Canvas.SetTop(rectangle, topPosition);
        }
    }
}