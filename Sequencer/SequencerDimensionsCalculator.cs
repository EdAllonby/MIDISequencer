using System;
using System.Windows;
using System.Windows.Controls;
using JetBrains.Annotations;

namespace Sequencer
{
    public sealed class SequencerDimensionsCalculator
    {
        private readonly Canvas sequencerCanvas;
        private readonly SequencerSettings sequencerSettings;

        public SequencerDimensionsCalculator([NotNull] Canvas sequencerCanvas, [NotNull] SequencerSettings sequencerSettings)
        {
            this.sequencerCanvas = sequencerCanvas;
            this.sequencerSettings = sequencerSettings;
        }

        /// <summary>
        /// The note heights the sequencer should display.
        /// </summary>
        public double NoteHeight => sequencerCanvas.ActualHeight/SequencerSettings.TotalNotes;

        /// <summary>
        /// The beat widths the sequencer should display.
        /// </summary>
        public double BeatWidth => sequencerCanvas.ActualWidth/(sequencerSettings.TimeSignature.BeatsPerMeasure*SequencerSettings.TotalMeasures);

        public Position FindNotePositionFromPoint(Point mousePosition)
        {
            var beat = (int)Math.Ceiling(mousePosition.X / BeatWidth);
            return Position.PositionFromBeat(beat, sequencerSettings.TimeSignature);
        }

        public Pitch FindPitch(Point mousePosition)
        {
            double noteHeight = NoteHeight;
            int relativeMidiNumber = (int)(sequencerCanvas.ActualHeight / noteHeight - Math.Ceiling(mousePosition.Y / noteHeight));
            int absoluteMidiNumber = sequencerSettings.LowestPitch.MidiNoteNumber + relativeMidiNumber;
            return Pitch.CreatePitchFromMidiNumber(absoluteMidiNumber);
        }
    }
}