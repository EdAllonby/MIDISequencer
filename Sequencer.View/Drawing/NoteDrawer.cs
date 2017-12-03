using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using JetBrains.Annotations;
using log4net;
using Sequencer.Domain;
using Sequencer.Shared;
using Sequencer.Utilities;
using Sequencer.View.Control;

namespace Sequencer.View.Drawing
{
    public sealed class NoteDrawer
    {
        [NotNull] private static readonly ILog Log = LogExtensions.GetLoggerSafe(typeof(NoteDrawer));

        [NotNull] private readonly Rectangle noteRectangle;
        [NotNull] private readonly ISequencerCanvasWrapper sequencer;
        [NotNull] private readonly ISequencerDimensionsCalculator sequencerDimensionsCalculator;
        [NotNull] private readonly SequencerSettings sequencerSettings;
        [NotNull] private readonly Rectangle velocityRectangle;

        public NoteDrawer([NotNull] ISequencerCanvasWrapper sequencer, [NotNull] SequencerSettings sequencerSettings,
            [NotNull] ISequencerDimensionsCalculator sequencerDimensionsCalculator)
        {
            this.sequencer = sequencer;
            this.sequencerSettings = sequencerSettings;
            this.sequencerDimensionsCalculator = sequencerDimensionsCalculator;

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
            Log.InfoFormat($"Drawing note length with start position {startPosition} to end position {endPosition}");

            double noteHeight = sequencerDimensionsCalculator.NoteHeight;

            double noteWidth = ActualWidthBetweenPositions(startPosition, endPosition);
            double noteStartHeight = sequencerDimensionsCalculator.GetPointFromPitch(pitch);

            noteRectangle.Height = noteHeight;
            noteRectangle.Width = noteWidth;

            SetNoteColour(noteState);

            double noteStartLocation = sequencerDimensionsCalculator.GetPointFromPosition(startPosition);

            SetRectanglePosition(noteRectangle, noteStartLocation, noteStartHeight);

            double velocityHeight = noteHeight * 0.3;
            double velocityStartHeight = noteStartHeight + noteHeight / 2 - velocityHeight / 2;

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
            sequencer.RemoveChild(noteRectangle);
            sequencer.RemoveChild(velocityRectangle);
        }

        private double ActualWidthBetweenPositions([NotNull] IPosition startPosition, [NotNull] IPosition endPosition)
        {
            double noteStartingPoint = sequencerDimensionsCalculator.GetPointFromPosition(startPosition);
            double noteEndingPoint = sequencerDimensionsCalculator.GetPointFromPosition(endPosition);
            double newNoteWidth = noteEndingPoint - noteStartingPoint;

            return newNoteWidth >= 0 ? newNoteWidth : 0;
        }

        private static void SetRectanglePosition([NotNull] UIElement rectangle, double leftPosition, double topPosition)
        {
            Canvas.SetLeft(rectangle, leftPosition);
            Canvas.SetTop(rectangle, topPosition);
        }
    }
}