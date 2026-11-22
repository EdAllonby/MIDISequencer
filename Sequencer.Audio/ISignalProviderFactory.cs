using JetBrains.Annotations;
using Sequencer.Domain;

namespace Sequencer.Audio
{
    public interface ISignalProviderFactory
    {
        [NotNull]
        ISignalProvider CreateSignalProvider([NotNull] ISequencedNote note);
    }
}
