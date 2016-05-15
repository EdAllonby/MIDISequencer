using System.Windows.Media;
using Sequencer.Domain;

namespace Sequencer
{
    /// <summary>
    /// Holds the sequencer's currently assigned settings.
    /// </summary>
    public sealed class SequencerSettings
    {
        // Musical definitions
        public const int TotalNotes = 32;
        public const int TotalMeasures = 4;
        private readonly Pitch lowestPitch = new Pitch(Note.A, 2);
        public readonly IDigitalAudioProtocol Protocol = new MidiProtocol();
        public readonly TimeSignature TimeSignature = new TimeSignature(4, 4);

        // Colour definitions
        public Color AccidentalKeyColour = Colors.Gray;
        public Color KeyColour = Colors.DarkGray;
        public Color LineColour = Colors.Black;
        public Color SelectedNoteColour = Colors.DarkRed;
        public Color UnselectedNoteColour = Colors.Crimson;

        /// <summary>
        /// Helper for finding the lower pitch protocol number with the currently set protocol.
        /// </summary>
        public int LowestPitchProtocolNumber => Protocol.ProtocolNoteNumber(lowestPitch);
    }
}