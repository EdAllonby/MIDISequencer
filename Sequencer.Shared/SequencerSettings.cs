using System.Windows.Media;
using JetBrains.Annotations;
using Sequencer.Domain;
using Sequencer.Midi;

namespace Sequencer.Shared
{
    /// <summary>
    /// Holds the sequencer's currently assigned settings.
    /// </summary>
    public sealed class SequencerSettings
    {
        // Musical definitions
        [NotNull] public const int TotalNotes = 32;

        [NotNull] public const int TotalMeasures = 4;
        [NotNull] public readonly Velocity DefaultVelocity = new Velocity(64);
        [NotNull] public readonly Pitch LowestPitch = new Pitch(Note.A, 2);
        [NotNull] public readonly IDigitalAudioProtocol Protocol = new MidiProtocol(new PitchAndPositionCalculator(TimeSignature.FourFour));
        [NotNull] public readonly TimeSignature TimeSignature = TimeSignature.FourFour;

        // Colour definitions
        public Color AccidentalKeyColour = Colors.Gray;
        public Color KeyColour = Colors.DarkGray;
        public Color LineColour = Colors.Black;
        public Color SelectedNoteColour = Colors.DarkRed;
        public Color UnselectedNoteColour = Colors.Crimson;
    }
}