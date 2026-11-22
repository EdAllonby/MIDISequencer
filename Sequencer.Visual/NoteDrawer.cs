using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using JetBrains.Annotations;
using log4net;
using Sequencer.Domain;
using Sequencer.Domain.Settings;
using Sequencer.Utilities;

namespace Sequencer.Visual
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
                Stroke = sequencerSettings.LineColour.ToBrush(),
                StrokeThickness = 0.5,
                RadiusX = 5,
                RadiusY = 5
            };

            velocityRectangle = new Rectangle { Fill = sequencerSettings.LineColour.ToBrush() };

            sequencer.AddChild(noteRectangle);
            sequencer.AddChild(velocityRectangle);

            // We always want the notes to be in the foreground.
            noteRectangle.ZIndex = 99;
            velocityRectangle.ZIndex = 100;
        }

        public void DrawNote([NotNull] Pitch pitch, [NotNull] Velocity velocity, [NotNull] IPosition startPosition, [NotNull] IPosition endPosition, NoteState noteState)
        {
            Log.DebugFormat($"Drawing note Pitch [{pitch}], Velocity [{velocity}] Start Position [{startPosition}] to End Position [{endPosition}]");

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
                    noteRectangle.Fill = sequencerSettings.SelectedNoteColour.ToBrush();
                    break;
                case NoteState.Unselected:
                    noteRectangle.Fill = sequencerSettings.UnselectedNoteColour.ToBrush();
                    break;
            }
        }

        public bool IntersectsWith(Rect rectangle)
        {
            var noteRect = new Rect(Canvas.GetLeft(noteRectangle), Canvas.GetTop(noteRectangle), noteRectangle.Width, noteRectangle.Height);

            return rectangle.Intersects(noteRect);
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

        private static void SetRectanglePosition([NotNull] Control rectangle, double leftPosition, double topPosition)
        {
            Canvas.SetLeft(rectangle, leftPosition);
            Canvas.SetTop(rectangle, topPosition);
        }
    }
}