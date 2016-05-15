using System.Windows;
using System.Windows.Controls;
using JetBrains.Annotations;
using log4net;
using Sequencer.Domain;
using Sequencer.Drawing;

namespace Sequencer
{
    public sealed class VisualNote : IPositionAware
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(VisualNote));

        [NotNull] private readonly NoteDrawer noteDrawer;
        private readonly IDigitalAudioProtocol protocol;
        private readonly SequencerSettings sequencerSettings;
        private Position endPosition;
        private NoteState noteState = NoteState.Selected;
        private Pitch pitch;
        private Position startPosition;

        public VisualNote([NotNull] SequencerDimensionsCalculator sequencerDimensionsCalculator, [NotNull] Canvas sequencer,
            [NotNull] SequencerSettings sequencerSettings, [NotNull] Position startPosition, [NotNull] Position endPosition, [NotNull] Pitch pitch)
        {
            protocol = sequencerSettings.Protocol;
            this.sequencerSettings = sequencerSettings;
            this.startPosition = startPosition;
            this.endPosition = endPosition;
            this.pitch = pitch;

            noteDrawer = new NoteDrawer(sequencer, sequencerSettings, sequencerDimensionsCalculator);
        }

        public Pitch Pitch
        {
            get { return pitch; }
            private set
            {
                pitch = value;
                Draw();
            }
        }

        /// <summary>
        /// The note's starting position.
        /// </summary>
        public Position StartPosition
        {
            get { return startPosition; }
            private set
            {
                startPosition = value;
                Draw();
            }
        }

        /// <summary>
        /// The note's ending position.
        /// </summary>
        public Position EndPosition
        {
            get { return endPosition; }
            set
            {
                if (!EndPosition.Equals(value) && (value > StartPosition))
                {
                    endPosition = value;
                    Draw();
                }
            }
        }

        /// <summary>
        /// The current state of the note.
        /// </summary>
        public NoteState NoteState
        {
            get { return noteState; }
            set
            {
                noteState = value;
                noteDrawer.SetNoteColour(noteState);
            }
        }

        public bool IntersectsWith(Rect rectangle)
        {
            return noteDrawer.IntersectsWith(rectangle);
        }

        public void Draw()
        {
            Log.InfoFormat("Drawing note length with start position {0} to end position {1}", StartPosition, EndPosition);
            noteDrawer.DrawNote(Pitch, StartPosition, EndPosition, noteState);
        }

        public void Remove()
        {
            noteDrawer.RemoveNote();
        }

        public override string ToString()
        {
            return $"Pitch: {Pitch}, Start Position: {StartPosition}, End Position: {EndPosition}";
        }

        public void MovePositionRelativeTo(int beatsToMove)
        {
            StartPosition = StartPosition.PositionRelativeByBeats(beatsToMove, sequencerSettings.TimeSignature);
            EndPosition = EndPosition.PositionRelativeByBeats(beatsToMove, sequencerSettings.TimeSignature);
        }

        public void MovePitchRelativeTo(int pitchesToMove)
        {
            int midiNoteNumber = protocol.ProtocolNoteNumber(pitch);
            Pitch = protocol.CreatePitchFromProtocolNumber(midiNoteNumber + pitchesToMove);
        }
    }
}