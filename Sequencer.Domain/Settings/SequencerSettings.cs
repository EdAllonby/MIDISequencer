﻿using System.Windows.Media;

namespace Sequencer.Domain.Settings
{
    /// <summary>
    /// Holds the sequencer's currently assigned settings.
    /// </summary>
    public sealed class SequencerSettings : IColourSettings, IMusicalSettings, IAudioSettings
    {
        // Audio definitions
        public int SampleRate => 44100;

        // Colour definitions
        public Color AccidentalKeyColour => Colors.Gray;

        public Color KeyColour => Colors.DarkGray;
        public Color LineColour => Colors.Black;
        public Color SelectedNoteColour => Colors.DarkRed;
        public Color UnselectedNoteColour => Colors.Crimson;

        public Color IndicatorColour => Colors.BurlyWood;

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