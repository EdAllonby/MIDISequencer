using JetBrains.Annotations;

namespace Sequencer.Domain
{
    /// <summary>
    /// A note placed in the sequence. Identity matters: playback tracks a sounding note by reference,
    /// so a note that is edited while it plays can still be released.
    /// </summary>
    public interface ISequencedNote
    {
        [NotNull]
        Tone Tone { get; }
    }
}
