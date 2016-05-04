using System;

namespace Sequencer
{
    /// <summary>
    /// A pitch combines a musical note and an octave.
    /// </summary>
    public sealed class Pitch : IEquatable<Pitch>
    {
        private readonly Note note;
        private readonly int octave;

        public Pitch(Note note, int octave)
        {
            this.note = note;
            this.octave = octave;
        }

        public int MidiNoteNumber => note.Value + (octave)*12;

        public static Pitch CreatePitchFromMidiNumber(int value)
        {
            int noteValue = value%12;

            int octave = (value - noteValue) / 12;

            var note = Note.FromValue(noteValue);

            return new Pitch(note, octave);
        }

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

            return note.Equals(other.note) && octave == other.octave;
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
                return (note.GetHashCode()*397) ^ octave;
            }
        }

        public override string ToString()
        {
            return $"{note}{octave}";
        }
    }
}