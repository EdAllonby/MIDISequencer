using JetBrains.Annotations;
using Sequencer.Domain;

namespace Sequencer.Shared
{
    public interface IMusicalSettings
    {
        [NotNull]
        int TotalNotes { get; }

        [NotNull]
        int TotalMeasures { get; }

        [NotNull]
        Velocity DefaultVelocity { get; }

        [NotNull]
        Pitch LowestPitch { get; }

        [NotNull]
        TimeSignature TimeSignature { get; }

        int TicksPerQuarterNote { get; }
    }
}