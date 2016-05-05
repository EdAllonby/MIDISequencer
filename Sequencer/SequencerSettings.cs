using System.Windows.Media;

namespace Sequencer
{
    /// <summary>
    /// Holds the sequencer's currently assigned settings.
    /// </summary>
    public sealed class SequencerSettings
    {
        // Musical definitions
        public readonly TimeSignature TimeSignature = new TimeSignature(4, 4);
        public readonly Pitch LowestPitch = new Pitch(Note.A, 2);
        public const int TotalNotes = 32;
        public const int TotalMeasures = 4;

        // Colour definitions
        public Color KeyColour = Colors.DarkGray;
        public Color AccidentalKeyColour = Colors.Gray;
        public Color LineColour = Colors.Black;
    }
}