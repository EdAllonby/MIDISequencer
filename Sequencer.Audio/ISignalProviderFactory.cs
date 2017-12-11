using JetBrains.Annotations;
using Sequencer.Visual;

namespace Sequencer.Audio
{
    public interface ISignalProviderFactory
    {
        [NotNull]
        ISignalProvider CreateSignalProvider([NotNull] IVisualNote visualNote);

    }
}