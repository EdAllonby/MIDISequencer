using JetBrains.Annotations;

namespace Sequencer.Domain
{
    public interface IFrequencyCalculator
    {
        double PitchFrequency([NotNull] Pitch pitch);
    }
}