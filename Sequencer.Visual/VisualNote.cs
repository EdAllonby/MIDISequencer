﻿using System;
using System.Windows.Media;
using JetBrains.Annotations;
using Sequencer.Domain;
using Sequencer.Domain.Settings;
using Sequencer.Midi;

namespace Sequencer.Visual
{
    public sealed class VisualNote : IVisualNote
    {
        [NotNull] private readonly NoteDrawer noteDrawer;
        [NotNull] private readonly IDigitalAudioProtocol protocol;
        [NotNull] private readonly SequencerSettings sequencerSettings;
        private bool canDraw = true;
        private NoteState noteState = NoteState.Selected;

        public VisualNote([NotNull] IDigitalAudioProtocol protocol, [NotNull] ISequencerDimensionsCalculator sequencerDimensionsCalculator,
            [NotNull] ISequencerCanvasWrapper sequencer, [NotNull] SequencerSettings sequencerSettings, [NotNull] Tone tone)
        {
            this.protocol = protocol;
            this.sequencerSettings = sequencerSettings;
            Tone = tone;

            noteDrawer = new NoteDrawer(sequencer, sequencerSettings, sequencerDimensionsCalculator);
        }

        public Pitch Pitch
        {
            get => Tone.Pitch;
            private set
            {
                Tone = new Tone(value, Tone.Velocity, Tone.StartPosition, Tone.EndPosition);
                Draw();
            }
        }

        public Tone Tone { get; private set; }

        public Velocity Velocity
        {
            get => Tone.Velocity;
            set
            {
                Tone = new Tone(Tone.Pitch, value, Tone.StartPosition, Tone.EndPosition);
                Draw();
            }
        }

        /// <summary>
        /// The note's starting position.
        /// </summary>
        public IPosition StartPosition
        {
            get => Tone.StartPosition;
            set
            {
                if (Tone.StartPosition.NextPosition(sequencerSettings.NoteResolution, sequencerSettings.TimeSignature, sequencerSettings.TicksPerQuarterNote).IsLessThan(Tone.EndPosition))
                {
                    Tone = new Tone(Tone.Pitch, Tone.Velocity, value, Tone.EndPosition);
                    Draw();
                }
            }
        }

        /// <summary>
        /// The note's ending position.
        /// </summary>
        public IPosition EndPosition
        {
            get => Tone.EndPosition;
            set
            {
                if (!EndPosition.Equals(value) && value.IsGreaterThan(StartPosition))
                {
                    Tone = new Tone(Tone.Pitch, Tone.Velocity, Tone.StartPosition, value);
                    Draw();
                }
            }
        }

        /// <summary>
        /// The current state of the note.
        /// </summary>
        public NoteState NoteState
        {
            get => noteState;
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
            if (canDraw)
            {
                noteDrawer.DrawNote(Pitch, Velocity, StartPosition, EndPosition, noteState);
            }
        }

        /// <summary>
        /// RemoveWithFade this visual note from its canvas.
        /// </summary>
        public void Remove()
        {
            noteDrawer.RemoveNote();
        }

        /// <summary>
        /// Moves the start and end positions of this visual note.
        /// </summary>
        /// <param name="ticksToMove">How many beats to move this visual note.</param>
        public void MovePositionRelativeTo(int ticksToMove)
        {
            DelayedDraw(() =>
            {
                StartPosition = StartPosition.PositionRelativeByTicks(ticksToMove, sequencerSettings.TimeSignature, sequencerSettings.TicksPerQuarterNote);
                EndPosition = EndPosition.PositionRelativeByTicks(ticksToMove, sequencerSettings.TimeSignature, sequencerSettings.TicksPerQuarterNote);
            });
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

        public override string ToString()
        {
            return $"Pitch: {Pitch}, Start IPosition: {StartPosition}, End IPosition: {EndPosition}";
        }

        private void DelayedDraw([NotNull] Action action)
        {
            canDraw = false;
            action();
            canDraw = true;
            Draw();
        }
    }
}