using System;
using JetBrains.Annotations;
using Sequencer.Domain;
using Sequencer.Midi;
using Sequencer.Shared;
using Sequencer.View.Control;
using Sequencer.View.Input;

namespace Sequencer.View.Drawing
{
    public sealed class SequencerDimensionsCalculator : ISequencerDimensionsCalculator
    {
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

        public bool IsPointInsideNote(ISequencerNotes sequencerNotes, IMousePoint mousePoint)
        {
            return FindNoteFromPoint(sequencerNotes, mousePoint) != null;
        }

        public IPosition FindPositionFromPoint(IMousePoint mousePosition)
        {
            var beat = (int) Math.Ceiling(mousePosition.X / BeatWidth);
            return Position.PositionFromBeat(beat, sequencerSettings.TimeSignature);
        }

        public double GetPointFromPosition(IPosition position)
        {
            return position.SummedBeat(sequencerSettings.TimeSignature) * BeatWidth - BeatWidth;
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
            IPosition mousePosition = FindPositionFromPoint(point);
            Pitch mousePitch = FindPitchFromPoint(point);
            return sequencerNotes.FindNoteFromPositionAndPitch(mousePosition, mousePitch);
        }

        public IVisualNote NoteAtStartingPoint(ISequencerNotes sequencerNotes, IMousePoint point)
        {
            IPosition mousePosition = FindPositionFromPoint(point);
            Pitch mousePitch = FindPitchFromPoint(point);

            return sequencerNotes.FindNoteFromStartingPositionAndPitch(mousePosition, mousePitch);
        }

        public IVisualNote NoteAtEndingPoint(ISequencerNotes sequencerNotes, IMousePoint point)
        {
            IPosition mousePosition = FindPositionFromPoint(point);
            Pitch mousePitch = FindPitchFromPoint(point);

            return sequencerNotes.FindNoteFromEndingPositionAndPitch(mousePosition, mousePitch);
        }
    }
}