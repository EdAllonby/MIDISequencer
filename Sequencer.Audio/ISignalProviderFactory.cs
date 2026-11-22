using JetBrains.Annotations;
using Sequencer.Domain;
using Sequencer.Domain.Settings;

namespace Sequencer.Audio
{
    public interface ISignalProviderFactory
    {
        /// <summary>
        /// A voice for a note, with the sound of the given instrument.
        /// </summary>
        [NotNull]
        ISignalProvider CreateSignalProvider([NotNull] ISequencedNote note, [NotNull] IAudioSettings instrument);
    }
}
