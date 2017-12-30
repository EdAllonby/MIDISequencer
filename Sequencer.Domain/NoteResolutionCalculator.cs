using System;

namespace Sequencer.Domain
{
    public static class NoteResolutionCalculator
    {
        public static int GetTicksForResolution(NoteResolution resolution, int ticksPerQuarterNote)
        {
            switch (resolution)
            {
                case NoteResolution.Whole:
                    return ticksPerQuarterNote * 4;
                case NoteResolution.Half:
                    return ticksPerQuarterNote * 2;
                case NoteResolution.Quarter:
                    return ticksPerQuarterNote;
                case NoteResolution.Eighth:
                    return ticksPerQuarterNote / 2;
                case NoteResolution.Sixteenth:
                    return ticksPerQuarterNote / 4;
                default:
                    throw new ArgumentOutOfRangeException(nameof(resolution), resolution, null);
            }
        }
    }
}