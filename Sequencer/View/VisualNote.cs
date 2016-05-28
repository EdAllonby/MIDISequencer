using System.Windows.Controls;
using System.Windows.Media;
using JetBrains.Annotations;
using Sequencer.Domain;
using Sequencer.Drawing;

namespace Sequencer.View
{
    public sealed class VisualNote : IPositionAware
    {
        [NotNull] private readonly NoteDrawer noteDrawer;
        private readonly IDigitalAudioProtocol protocol;
        private readonly SequencerSettings sequencerSettings;
        private Position endPosition;
        private NoteState noteState = NoteState.Selected;
        private Pitch pitch;
        private Position startPosition;
        private Velocity velocity;

        public VisualNote([NotNull] SequencerDimensionsCalculator sequencerDimensionsCalculator, [NotNull] Canvas sequencer,
            [NotNull] SequencerSettings sequencerSettings, [NotNull] Velocity velocity,
            [NotNull] Position startPosition, [NotNull] Position endPosition, [NotNull] Pitch pitch)
        {
            protocol = sequencerSettings.Protocol;
            this.sequencerSettings = sequencerSettings;
            this.velocity = velocity;
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

        public Velocity Velocity
        {
            get { return velocity; }
            set
            {
                velocity = value;
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
                if (value != null)
                {
                    startPosition = value;
                    Draw();
                }
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
                if ((value != null) && !EndPosition.Equals(value) && (value > StartPosition))
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

        /// <summary>
        /// Checks if this visual note intersects with another rectangle.
        /// </summary>
        /// <param name="geometry">The rectangle to check if this visual note intersects.</param>
        /// <returns>If this visual note intersects with the rectangle.</returns>
        public bool IntersectsWith(Geometry geometry)
        {
            return noteDrawer.IntersectsWith(geometry.Bounds);
        }

        /// <summary>
        /// Draws this visual note onto its canvas.
        /// </summary>
        public void Draw()
        {
            noteDrawer.DrawNote(Pitch, Velocity, StartPosition, EndPosition, noteState);
        }

        /// <summary>
        /// RemoveWithFade this visual note from its canvas.
        /// </summary>
        public void Remove()
        {
            noteDrawer.RemoveNote();
        }

        public override string ToString()
        {
            return $"Pitch: {Pitch}, Start Position: {StartPosition}, End Position: {EndPosition}";
        }

        /// <summary>
        /// Moves the start and end positions of this visual note.
        /// </summary>
        /// <param name="beatsToMove">How many beats to move this visual note.</param>
        public void MovePositionRelativeTo(int beatsToMove)
        {
            StartPosition = StartPosition.PositionRelativeByBeats(beatsToMove, sequencerSettings.TimeSignature);
            EndPosition = EndPosition.PositionRelativeByBeats(beatsToMove, sequencerSettings.TimeSignature);
        }

        /// <summary>
        /// Moves the pitch of this visual note.
        /// </summary>
        /// <param name="halfStepsToMove">How many half steps to move this visual note.</param>
        public void MovePitchRelativeTo(int halfStepsToMove)
        {
            int midiNoteNumber = protocol.ProtocolNoteNumber(pitch);
            Pitch = protocol.CreatePitchFromProtocolNumber(midiNoteNumber + halfStepsToMove);
        }
    }
}