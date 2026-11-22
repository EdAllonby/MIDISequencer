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

        // Envelope values match the original Windows build. Its release stage never ran (issue #16), so
        // the one-second release tail is new.
        public double AttackSeconds => 1.0;
        public double DecaySeconds => 1.0;
        public double SustainLevel => 0.5;
        public double ReleaseSeconds => 1.0;

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