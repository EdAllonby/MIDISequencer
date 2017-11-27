using System;
using System.Windows;
using System.Windows.Controls;
using JetBrains.Annotations;
using Sequencer.Domain;
using Sequencer.View;

namespace Sequencer.Drawing
{
    public sealed class SequencerDimensionsCalculator
    {
        private readonly IDigitalAudioProtocol protocol;
        private readonly Canvas sequencerCanvas;
        private readonly SequencerSettings sequencerSettings;

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

        public bool IsPointInsideNote(SequencerNotes sequencerNotes, Point mousePoint)
        {
            return FindNoteFromPoint(sequencerNotes, mousePoint) != null;
        }

        public Position FindPositionFromPoint(Point mousePosition)
        {
            var beat = (int) Math.Ceiling(mousePosition.X/BeatWidth);
            return Position.PositionFromBeat(beat, sequencerSettings.TimeSignature);
        }

        /// <summary>
        /// Finds the pitch a mouse is on from a position in the sequencer.
        /// </summary>
        /// <param name="mousePosition">The position the mouse is relative to the sequencer.</param>
        /// <returns>The pitch the mouse is over.</returns>
        public Pitch FindPitchFromPoint(Point mousePosition)
        {
            int relativeMidiNumber = (int) ((sequencerCanvas.ActualHeight/NoteHeight) - Math.Ceiling(mousePosition.Y/NoteHeight));
            int absoluteMidiNumber = protocol.ProtocolNoteNumber(sequencerSettings.LowestPitch) + relativeMidiNumber;
            return protocol.CreatePitchFromProtocolNumber(absoluteMidiNumber);
        }

        /// <summary>        /// Finds a visual note relative to the sequencer.        /// </summary>        /// <param name="sequencerNotes">The notes in the sequencer.</param>        /// <param name="point">The position the mouse is relative to the sequencer.</param>        /// <returns>The note a mouse is over.</returns>        public VisualNote FindNoteFromPoint([NotNull] SequencerNotes sequencerNotes, Point point)        {            Position mousePosition = FindPositionFromPoint(point);            Pitch mousePitch = FindPitchFromPoint(point);            return sequencerNotes.FindNoteFromPositionAndPitch(mousePosition, mousePitch);        }        public VisualNote NoteAtStartingPoint(SequencerNotes sequencerNotes, Point point)
        {
            Position mousePosition = FindPositionFromPoint(point);
            Pitch mousePitch = FindPitchFromPoint(point);

            return sequencerNotes.FindNoteFromStartingPositionAndPitch(mousePosition, mousePitch);
        }

        public VisualNote NoteAtEndingPoint(SequencerNotes sequencerNotes, Point point)
        {
            Position mousePosition = FindPositionFromPoint(point);
            Pitch mousePitch = FindPitchFromPoint(point);

            return sequencerNotes.FindNoteFromEndingPositionAndPitch(mousePosition, mousePitch);
        }
    }
}