using System.Windows.Media;
using Sequencer.Domain;

namespace Sequencer.Shared
{
    /// <summary>
    /// Holds the sequencer's currently assigned settings.
    /// </summary>
    public sealed class SequencerSettings : IColourSettings, IMusicalSettings
    {
        // Colour definitions
        public Color AccidentalKeyColour => Colors.Gray;

        public Color KeyColour => Colors.DarkGray;
        public Color LineColour => Colors.Black;
        public Color SelectedNoteColour => Colors.DarkRed;
        public Color UnselectedNoteColour => Colors.Crimson;

        public Color IndicatorColour => Colors.BurlyWood;

        // Musical definitions
        public int TotalNotes => 32;
        public int TotalMeasures => 1;
        public Velocity DefaultVelocity => new Velocity(64);
        public Pitch LowestPitch => new Pitch(Note.A, 2);
        public TimeSignature TimeSignature => TimeSignature.FourFour;
        public int TicksPerQuarterNote => 96;
    }
}