using JetBrains.Annotations;
using Sequencer.Domain;
using Sequencer.Shared;

namespace Sequencer.Midi
{
    public class TickCalculator : ITickCalculator
    {
        [NotNull] private readonly TimeSignature timeSignature;
        private readonly int quarterNoteResolution;

        public TickCalculator([NotNull] IMusicalSettings musicalSettings)
        {
            // TODO: Inject
            timeSignature = musicalSettings.TimeSignature;
            quarterNoteResolution = musicalSettings.TicksPerQuarterNote;
        }

        public IPosition CalculatePositionFromTick(int tick)
        {
            int currentBeat = (tick + quarterNoteResolution) / quarterNoteResolution;

            return Position.PositionFromBeat(currentBeat, timeSignature);
        }
    }
}