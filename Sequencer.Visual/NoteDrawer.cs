using System;
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
                StrokeThickness = 1,
                RadiusX = 2,
                RadiusY = 2
            };

            // A thin strip along the bottom of the note shows its velocity.
            velocityRectangle = new Rectangle { Fill = sequencerSettings.VelocityColour.ToBrush(), RadiusX = 1, RadiusY = 1, IsHitTestVisible = false };

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

            // Inset by half a pixel so the outline sits on whole pixels and touching notes stay distinct.
            double noteStartLocation = sequencerDimensionsCalculator.GetPointFromPosition(startPosition);
            noteRectangle.Width = Math.Max(0, noteWidth - 1);
            noteRectangle.Height = Math.Max(0, noteHeight - 1);
            SetRectanglePosition(noteRectangle, Math.Floor(noteStartLocation) + 0.5, Math.Floor(noteStartHeight) + 0.5);

            SetNoteColour(noteState);

            const double velocityHeight = 2;
            const double velocityInset = 2;
            velocityRectangle.Height = velocityHeight;
            velocityRectangle.Width = Math.Max(0, (noteWidth - velocityInset * 2 - 1) * velocity.Volume);
            SetRectanglePosition(velocityRectangle, Math.Floor(noteStartLocation) + velocityInset,
                Math.Floor(noteStartHeight) + noteHeight - velocityInset - velocityHeight);
        }

        public void SetNoteColour(NoteState noteState)
        {
            switch (noteState)
            {
                case NoteState.Selected:
                    noteRectangle.Fill = sequencerSettings.SelectedNoteColour.ToBrush();
                    noteRectangle.Stroke = sequencerSettings.SelectedNoteOutlineColour.ToBrush();
                    break;
                case NoteState.Unselected:
                    noteRectangle.Fill = sequencerSettings.NoteColour.ToBrush();
                    noteRectangle.Stroke = sequencerSettings.NoteOutlineColour.ToBrush();
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