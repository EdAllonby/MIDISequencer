using System;
using System.Windows.Controls;
using JetBrains.Annotations;
using Sequencer.Domain;
using Sequencer.Midi;
using Sequencer.Shared;
using Sequencer.View.Input;
using Sequencer.View.Control;

namespace Sequencer.View.Drawing
{
    public sealed class SequencerDimensionsCalculator : ISequencerDimensionsCalculator
    {
        [NotNull] private readonly IDigitalAudioProtocol protocol;
        [NotNull] private readonly Canvas sequencerCanvas;
        [NotNull] private readonly SequencerSettings sequencerSettings;

        public SequencerDimensionsCalculator([NotNull] Canvas sequencerCanvas, [NotNull] SequencerSettings sequencerSettings)
        {
            protocol = sequencerSettings.Protocol;
            this.sequencerCanvas = sequencerCanvas;
            this.sequencerSettings = sequencerSettings;
        }

        /// <summary>
        /// The note heights the sequencer should display.
        /// </summary>
        public double NoteHeight => sequencerCanvas.ActualHeight/SequencerSettings.TotalNotes;

        /// <summary>
        /// The measure widths the sequencer should display.
        /// </summary>
        public double MeasureWidth => sequencerCanvas.ActualWidth/SequencerSettings.TotalMeasures;

        /// <summary>
        /// The brr widths the sequencer should display.
        /// </summary>
        public double BarWidth => MeasureWidth/sequencerSettings.TimeSignature.BarsPerMeasure;

        /// <summary>
        /// The beat widths the sequencer should display.
        /// </summary>
        public double BeatWidth => BarWidth/sequencerSettings.TimeSignature.BeatsPerBar;

        public bool IsPointInsideNote(ISequencerNotes sequencerNotes, IMousePoint mousePoint)
        {
            return FindNoteFromPoint(sequencerNotes, mousePoint) != null;
        }

        public IPosition FindPositionFromPoint(IMousePoint mousePosition)
        {
            var beat = (int) Math.Ceiling(mousePosition.X/BeatWidth);
            return Position.PositionFromBeat(beat, sequencerSettings.TimeSignature);
        }

        /// <summary>
        /// Finds the pitch a mouse is on from a position in the sequencer.
        /// </summary>
        /// <param name="mousePosition">The position the mouse is relative to the sequencer.</param>
        /// <returns>The pitch the mouse is over.</returns>
        public Pitch FindPitchFromPoint(IMousePoint mousePosition)
        {
            int relativeMidiNumber = (int) ((sequencerCanvas.ActualHeight/NoteHeight) - Math.Ceiling(mousePosition.Y/NoteHeight));
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