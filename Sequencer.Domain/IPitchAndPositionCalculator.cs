using JetBrains.Annotations;

namespace Sequencer.Domain
{
    public interface IPitchAndPositionCalculator
    {
        [Pure]
        int FindStepsFromPitches([NotNull] Pitch firstPitch, [NotNull] Pitch secondPitch);

        [Pure]
        int FindBeatsBetweenPositions([NotNull] IPosition initialPosition, [NotNull] IPosition newPosition);
    }
}