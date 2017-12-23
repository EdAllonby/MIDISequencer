using JetBrains.Annotations;

namespace Sequencer.Domain
{
    public interface IPitchAndPositionCalculator
    {
        [Pure]
        int FindStepsFromPitches([NotNull] Pitch firstPitch, [NotNull] Pitch secondPitch);

        [Pure]
        int FindTicksBetweenPositions([NotNull] IPosition initialPosition, [NotNull] IPosition newPosition);
    }
}