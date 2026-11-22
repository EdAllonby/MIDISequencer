using System.Drawing;

namespace Sequencer.Domain.Settings
{
    /// <summary>
    /// Holds the sequencer's currently assigned settings.
    /// </summary>
    public sealed class SequencerSettings : IColourSettings, IMusicalSettings, IAudioSettings
    {
        // Audio definitions
        public int SampleRate => 44100;

        // A short, percussive envelope. The original Windows build used 1 s attack, decay and release
        // with 0.5 sustain, but its release stage never ran (issue #16).
        public double AttackSeconds => 0.005;
        public double DecaySeconds => 0.1;
        public double SustainLevel => 0.7;
        public double ReleaseSeconds => 0.2;

        // Colour definitions
        public Color AccidentalKeyColour => Color.Gray;

        public Color KeyColour => Color.DarkGray;
        public Color LineColour => Color.Black;
        public Color SelectedNoteColour => Color.DarkRed;
        public Color UnselectedNoteColour => Color.Crimson;

        public Color IndicatorColour => Color.BurlyWood;

        // Musical definitions
        public int TotalNotes => 64;

        public int TotalMeasures => 1;
        public Velocity DefaultVelocity => new Velocity(64);
        public Pitch LowestPitch => new Pitch(Note.A, 1);
        public TimeSignature TimeSignature => TimeSignature.FourFour;
        public int TicksPerQuarterNote => 96;
        public NoteResolution NoteResolution => NoteResolution.Sixteenth;
    }
}