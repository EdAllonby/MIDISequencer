using JetBrains.Annotations;
using Sequencer.Domain;
using Sequencer.Midi;

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
        IDigitalAudioProtocol Protocol { get; }

        [NotNull]
        TimeSignature TimeSignature { get; }

    }
}