using System.Drawing;

namespace Sequencer.Domain.Settings
{
    /// <summary>
    /// Holds the sequencer's currently assigned settings.
    /// </summary>
    public sealed class SequencerSettings : IColourSettings, IMusicalSettings
    {
        // Colour definitions: a dark "studio" palette. Notes are the brightest thing on the canvas; the grid
        // recedes, with bars stronger than beats and beats stronger than sixteenths.
        public Color LaneColour => Hex(0x24262B);
        public Color AccidentalLaneColour => Hex(0x1D1F23);

        public Color SixteenthLineColour => Hex(0x2B2E33);
        public Color BeatLineColour => Hex(0x363940);
        public Color BarLineColour => Hex(0x50545D);
        public Color OctaveLineColour => Hex(0x41454D);

        public Color NoteColour => Hex(0xE8A33D);
        public Color SelectedNoteColour => Hex(0xFFD48A);
        public Color NoteOutlineColour => Hex(0x14151A);
        public Color SelectedNoteOutlineColour => Hex(0xFFFFFF);
        public Color VelocityColour => Color.FromArgb(110, 20, 21, 26);

        public Color PlayheadColour => Hex(0xF2F3F5);
        public Color ActiveKeyColour => Hex(0xE8A33D);

        public Color WhiteKeyColour => Hex(0xC9CCD1);
        public Color BlackKeyColour => Hex(0x26282D);
        public Color KeyLabelColour => Hex(0x5D6168);

        public Color RulerTextColour => Hex(0x9A9EA5);
        public Color RulerTickColour => Hex(0x4A4E56);

        public Color SelectionColour => Hex(0xE8A33D);

        private static Color Hex(int rgb) => Color.FromArgb(255, (rgb >> 16) & 0xFF, (rgb >> 8) & 0xFF, rgb & 0xFF);

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