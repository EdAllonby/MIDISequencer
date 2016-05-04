namespace Sequencer
{
    /// <summary>
    /// Represents an 'atomic' note without duration.
    /// </summary>
    public class Note : EnumerableType<Note>
    {
        public static readonly Note C = new Note(0, "C", false);
        public static readonly Note CSharp = new Note(1, "C#", true);
        public static readonly Note D = new Note(2, "D", false);
        public static readonly Note DSharp = new Note(3, "D#", true);
        public static readonly Note E = new Note(4, "E", false);
        public static readonly Note F = new Note(5, "F", false);
        public static readonly Note FSharp = new Note(6, "F#", true);
        public static readonly Note G = new Note(7, "G", false);
        public static readonly Note GSharp = new Note(8, "G#", true);
        public static readonly Note A = new Note(9, "A", false);
        public static readonly Note ASharp = new Note(10, "A#", true);
        public static readonly Note B = new Note(11, "B", false);

        private Note(int value, string displayName, bool isAccidental) : base(value, displayName)
        {
            IsAccidental = isAccidental;
        }

        /// <summary>
        /// Whether this note is 'accidental', i.e. is a sharp or flat note.
        /// </summary>
        public bool IsAccidental { get; }
    }
}