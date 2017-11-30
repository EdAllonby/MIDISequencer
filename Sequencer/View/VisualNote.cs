using System.Windows.Media;
using JetBrains.Annotations;
using Sequencer.Domain;
using Sequencer.Drawing;

namespace Sequencer.View
{
    public sealed class VisualNote : IVisualNote
    {
        [NotNull] private readonly NoteDrawer noteDrawer;
        private readonly IDigitalAudioProtocol protocol;
        private readonly SequencerSettings sequencerSettings;
        private NoteState noteState = NoteState.Selected;

        public VisualNote([NotNull] ISequencerDimensionsCalculator sequencerDimensionsCalculator, [NotNull] ISequencerCanvasWrapper sequencer, [NotNull] SequencerSettings sequencerSettings, Tone tone)
        {
            protocol = sequencerSettings.Protocol;
            this.sequencerSettings = sequencerSettings;
            Tone = tone;

            noteDrawer = new NoteDrawer(sequencer, sequencerSettings, sequencerDimensionsCalculator);
        }

        public Pitch Pitch
        {
            get { return Tone.Pitch; }
            private set
            {
                Tone.Pitch = value;
                Draw();
            }
        }

        public Tone Tone { get; }

        public Velocity Velocity
        {
            get { return Tone.Velocity; }
            set
            {
                Tone.Velocity = value;
                Draw();
            }
        }

        /// <summary>
        /// The note's starting position.
        /// </summary>
        public IPosition StartPosition
        {
            get { return Tone.StartPosition; }
            set
            {
                if (value != null)
                {
                    if (Tone.StartPosition.NextPosition(sequencerSettings.TimeSignature).IsLessThan(Tone.EndPosition))
                    {
                        Tone.StartPosition = value;
                        Draw();
                    }
                }
            }
        }

        /// <summary>
        /// The note's ending position.
        /// </summary>
        public IPosition EndPosition
        {
            get { return Tone.EndPosition; }
            set
            {
                if ((value != null) && !EndPosition.Equals(value) && (value.IsGreaterThan(StartPosition)))
                {
                    Tone.EndPosition = value;
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
            return $"Pitch: {Pitch}, Start IPosition: {StartPosition}, End IPosition: {EndPosition}";
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
            int midiNoteNumber = protocol.ProtocolNoteNumber(Tone.Pitch);
            Pitch = protocol.CreatePitchFromProtocolNumber(midiNoteNumber + halfStepsToMove);
        }
    }
}