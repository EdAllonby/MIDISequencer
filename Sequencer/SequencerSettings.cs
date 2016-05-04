namespace Sequencer
{
    /// <summary>
    /// Holds the sequencer's currently assigned settings.
    /// </summary>
    public sealed class SequencerSettings
    {
        public readonly TimeSignature TimeSignature = new TimeSignature(4, 4);
        public readonly Pitch LowestPitch = new Pitch(Note.A, 2);
        public const int TotalNotes = 32;
        public const int TotalMeasures = 4;
    }
}