using JetBrains.Annotations;
using Sequencer.Domain;

namespace Sequencer.Midi
{
    /// <summary>
    /// A digital audio protocol defines a set of rules for representing musical values as digital data.
    /// </summary>
    public interface IDigitalAudioProtocol
    {
        /// <summary>
        /// Calculates the protocol number for a pitch.
        /// </summary>
        /// <param name="pitch">The pitch to find the protocol number for.</param>
        /// <returns>The protocol number for the pitch.</returns>
        int ProtocolNoteNumber([NotNull] Pitch pitch);

        /// <summary>
        /// Creates a pitch from a protocol number.
        /// </summary>
        /// <param name="protocolNumber">The protocol number to find the pitch for.</param>
        /// <returns>The pitch from the protocol number.</returns>
        [NotNull]
        Pitch CreatePitchFromProtocolNumber(int protocolNumber);
    }
}