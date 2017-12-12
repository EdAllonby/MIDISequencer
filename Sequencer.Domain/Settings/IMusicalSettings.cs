using JetBrains.Annotations;

namespace Sequencer.Domain.Settings
{
    public interface IMusicalSettings
    {
        int TotalNotes { get; }

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