using JetBrains.Annotations;

namespace Sequencer.Domain
{
    /// <summary>
    /// Represents an 'atomic' note without duration.
    /// </summary>
    public class Note : EnumerableType<Note>
    {
        [UsedImplicitly] [NotNull] public static readonly Note C = new Note(0, "C", false);

        [UsedImplicitly] [NotNull] public static readonly Note CSharp = new Note(1, "C#", true);

        [UsedImplicitly] [NotNull] public static readonly Note D = new Note(2, "D", false);

        [UsedImplicitly] [NotNull] public static readonly Note DSharp = new Note(3, "D#", true);

        [UsedImplicitly] [NotNull] public static readonly Note E = new Note(4, "E", false);

        [UsedImplicitly] [NotNull] public static readonly Note F = new Note(5, "F", false);

        [UsedImplicitly] [NotNull] public static readonly Note FSharp = new Note(6, "F#", true);

        [UsedImplicitly] [NotNull] public static readonly Note G = new Note(7, "G", false);

        [UsedImplicitly] [NotNull] public static readonly Note GSharp = new Note(8, "G#", true);

        [UsedImplicitly] [NotNull] public static readonly Note A = new Note(9, "A", false);

        [UsedImplicitly] [NotNull] public static readonly Note ASharp = new Note(10, "A#", true);

        [UsedImplicitly] [NotNull] public static readonly Note B = new Note(11, "B", false);

        private Note(int value, [NotNull] string displayName, bool isAccidental) : base(value, displayName)
        {
            IsAccidental = isAccidental;
        }

        /// <summary>
        /// Whether this note is 'accidental', i.e. is a sharp or flat note.
        /// </summary>
        public bool IsAccidental { get; }
    }
}