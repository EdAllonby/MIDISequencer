using System;
using JetBrains.Annotations;

namespace Sequencer.Domain
{
    /// <summary>
    /// A pitch combines a musical note and an octave.
    /// </summary>
    public sealed class Pitch : IEquatable<Pitch>
    {
        /// <summary>
        /// Create a new pitch with a note and and octave.
        /// </summary>
        /// <param name="note">The note of this pitch.</param>
        /// <param name="octave">The octave of this pitch.</param>
        public Pitch([NotNull] Note note, int octave)
        {
            Note = note;
            Octave = octave;
        }

        /// <summary>
        /// The octave this pitch is in.
        /// </summary>
        public int Octave { get; }

        /// <summary>
        /// The note this pitch has.
        /// </summary>
        [NotNull]
        public Note Note { get; }

        public bool Equals(Pitch other)
        {
            if (other is null) return false;
            if (ReferenceEquals(this, other)) return true;

            return Note.Equals(other.Note) && Octave == other.Octave;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj is Pitch pitch && Equals(pitch);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (Note.GetHashCode() * 397) ^ Octave;
            }
        }

        public override string ToString()
        {
            return $"{Note}{Octave}";
        }
    }
}