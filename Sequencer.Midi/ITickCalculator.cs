using JetBrains.Annotations;
using Sequencer.Domain;

namespace Sequencer.Midi
{
    public interface ITickCalculator
    {
        [NotNull]
        IPosition CalculatePositionFromTick(int tick, int quaterNoteResolution);
    }
}