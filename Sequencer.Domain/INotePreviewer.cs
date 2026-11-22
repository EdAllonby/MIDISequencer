using JetBrains.Annotations;

namespace Sequencer.Domain
{
    /// <summary>
    /// Plays a short preview of a note, for feedback while editing.
    /// </summary>
    public interface INotePreviewer
    {
        void Preview([NotNull] Tone tone);
    }
}
