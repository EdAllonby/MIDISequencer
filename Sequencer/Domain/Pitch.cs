using System;
using JetBrains.Annotations;

namespace Sequencer.Domain
{
    /// <summary>
    /// A pitch combines a musical note and an octave.
    /// </summary>
    public sealed class Pitch : IEquatable<Pitch>
    {
        private readonly int octave;

        public Pitch([NotNull] Note note, int octave)
        {
            Note = note;
            this.octave = octave;
        }

        [NotNull]
        public Note Note { get; }

        /// <summary>
        /// The MIDI Equivalent of this pitch.
        /// </summary>
        public int MidiNoteNumber => Note.Value + octave*12;

        public bool Equals(Pitch other)
        {
            if (ReferenceEquals(null, other))
            {
                return false;
            }
            if (ReferenceEquals(this, other))
            {
                return true;
            }

            return Note.Equals(other.Note) && octave == other.octave;
        }

        public static Pitch CreatePitchFromMidiNumber(int value)
        {
            int noteValue = value%12;

            int octave = (value - noteValue)/12;

            Note note = Note.FromValue(noteValue);

            return new Pitch(note, octave);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj is Pitch && Equals((Pitch) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (Note.GetHashCode()*397) ^ octave;
            }
        }

        public override string ToString()
        {
            return $"{Note}{octave}";
        }
    }
}