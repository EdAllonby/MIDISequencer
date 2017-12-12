using JetBrains.Annotations;
using Sequencer.Domain;
using Sequencer.Domain.Settings;

namespace Sequencer.Midi
{
    public class TickCalculator : ITickCalculator
    {
        private readonly int quarterNoteResolution;
        [NotNull] private readonly TimeSignature timeSignature;

        public TickCalculator([NotNull] IMusicalSettings musicalSettings)
        {
            timeSignature = musicalSettings.TimeSignature;
            quarterNoteResolution = musicalSettings.TicksPerQuarterNote;
        }

        public IPosition CalculatePositionFromTick(int tick)
        {
            int currentBeat = (tick + quarterNoteResolution) / quarterNoteResolution;

            int wrappedTick = tick % quarterNoteResolution;
            return Position.PositionFromBeat(currentBeat, wrappedTick, timeSignature);
        }
    }
}