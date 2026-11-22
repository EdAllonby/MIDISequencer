using System;
using JetBrains.Annotations;

namespace Sequencer.Domain
{
    /// <summary>
    /// Plays notes outside the sequence: previews while editing, and keys played on the piano keyboard.
    /// </summary>
    public interface INotePreviewer
    {
        /// <summary>
        /// A short preview, if the user has editing previews switched on.
        /// </summary>
        void Preview([NotNull] Tone tone);

        /// <summary>
        /// Starts the note and holds it until the returned handle is disposed. Always plays.
        /// </summary>
        [NotNull]
        IDisposable Hold([NotNull] Tone tone);
    }
}
