using System;
using System.Linq;
using Avalonia;
using JetBrains.Annotations;
using Sequencer.Domain;
using Sequencer.Domain.Settings;
using Sequencer.Midi;
using Sequencer.Utilities;
using Sequencer.Visual;
using Sequencer.Visual.Input;

namespace Sequencer.View.Drawing
{
    public sealed class SequencerDimensionsCalculator : ISequencerDimensionsCalculator
    {
        private const double MaximumEdgeWidth = 6;

        [NotNull] private readonly IPitchAndPositionCalculator pitchAndPositionCalculator;
        [NotNull] private readonly IDigitalAudioProtocol protocol;
        [NotNull] private readonly ISequencerCanvasWrapper sequencerCanvas;
        [NotNull] private readonly SequencerSettings sequencerSettings;

        public SequencerDimensionsCalculator([NotNull] IDigitalAudioProtocol protocol, [NotNull] ISequencerCanvasWrapper sequencerCanvas,
            [NotNull] SequencerSettings sequencerSettings, [NotNull] IPitchAndPositionCalculator pitchAndPositionCalculator)
        {
            this.protocol = protocol;
            this.sequencerCanvas = sequencerCanvas;
            this.sequencerSettings = sequencerSettings;
            this.pitchAndPositionCalculator = pitchAndPositionCalculator;
        }

        private double TickWidth => BeatWidth / sequencerSettings.TicksPerQuarterNote;

        /// <summary>
        /// The note heights the sequencer should display.
        /// </summary>
        public double NoteHeight => sequencerCanvas.Height / sequencerSettings.TotalNotes;

        /// <summary>
        /// The measure widths the sequencer should display.
        /// </summary>
        public double MeasureWidth => sequencerCanvas.Width / sequencerSettings.TotalMeasures;

        /// <summary>
        /// The brr widths the sequencer should display.
        /// </summary>
        public double BarWidth => MeasureWidth / sequencerSettings.TimeSignature.BarsPerMeasure;

        /// <summary>
        /// The beat widths the sequencer should display.
        /// </summary>
        public double BeatWidth => BarWidth / sequencerSettings.TimeSignature.BeatsPerBar;

        public double SixteenthNoteWidth => BeatWidth / 4;

        public bool IsPointInsideNote(ISequencerNotes sequencerNotes, IMousePoint mousePoint)
        {
            return FindNoteFromPoint(sequencerNotes, mousePoint) != null;
        }

        public IPosition FindPositionFromPoint(IMousePoint mousePosition)
        {
            // The start of the grid cell containing the point. (Rounding to the nearest grid line put notes
            // in the next cell when clicking the right half of a cell.)
            int resolutionTicks = NoteResolutionCalculator.GetTicksForResolution(sequencerSettings.NoteResolution, sequencerSettings.TicksPerQuarterNote);
            var tick = (int) Math.Floor(Math.Max(0, mousePosition.X) / TickWidth);
            int cellStartTick = tick / resolutionTicks * resolutionTicks;

            return Position.PositionFromTick(cellStartTick, sequencerSettings.TimeSignature, sequencerSettings.TicksPerQuarterNote);
        }

        public double GetPointFromPosition(IPosition position)
        {
            // TODO: Tests
            return TickWidth * position.TotalTicks(sequencerSettings.TimeSignature, sequencerSettings.TicksPerQuarterNote);
        }

        public double GetPointFromPitch(Pitch pitch)
        {
            int halfStepDifference = pitchAndPositionCalculator.FindStepsFromPitches(sequencerSettings.LowestPitch, pitch);
            double relativePitchPosition = NoteHeight * halfStepDifference + NoteHeight;
            return sequencerCanvas.Height - relativePitchPosition;
        }

        /// <summary>
        /// Finds the pitch a mouse is on from a position in the sequencer.
        /// </summary>
        /// <param name="mousePosition">The position the mouse is relative to the sequencer.</param>
        /// <returns>The pitch the mouse is over.</returns>
        public Pitch FindPitchFromPoint(IMousePoint mousePosition)
        {
            var relativeMidiNumber = (int) (sequencerCanvas.Height / NoteHeight - Math.Ceiling(mousePosition.Y / NoteHeight));
            int absoluteMidiNumber = protocol.ProtocolNoteNumber(sequencerSettings.LowestPitch) + relativeMidiNumber;
            return protocol.CreatePitchFromProtocolNumber(absoluteMidiNumber);
        }

        /// <summary>
        /// Finds a visual note relative to the sequencer.
        /// </summary>
        /// <param name="sequencerNotes">The notes in the sequencer.</param>
        /// <param name="point">The position the mouse is relative to the sequencer.</param>
        /// <returns>The note a mouse is over.</returns>
        public IVisualNote FindNoteFromPoint(ISequencerNotes sequencerNotes, IMousePoint point)
        {
            // Hit-test the drawn rectangle. Snapping the point to the grid first (as this used to) misses the
            // right half of a one-sixteenth note.
            return sequencerNotes.AllNotes.LastOrDefault(note => NoteBounds(note).Contains(point.Point));
        }

        public NoteEdge EdgeAtPoint(IVisualNote note, IMousePoint point)
        {
            Rect bounds = NoteBounds(note);

            // Up to 6 px at each end resizes; short notes keep at least half their width for moving.
            double edgeWidth = Math.Min(MaximumEdgeWidth, bounds.Width / 4);

            if (point.X >= bounds.Right - edgeWidth)
            {
                return NoteEdge.End;
            }

            if (point.X <= bounds.Left + edgeWidth)
            {
                return NoteEdge.Start;
            }

            return NoteEdge.None;
        }

        private Rect NoteBounds([NotNull] IVisualNote note)
        {
            double left = GetPointFromPosition(note.StartPosition);
            double right = GetPointFromPosition(note.EndPosition);
            return new Rect(left, GetPointFromPitch(note.Pitch), Math.Max(0, right - left), NoteHeight);
        }
    }
}