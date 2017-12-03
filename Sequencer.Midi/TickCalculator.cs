using JetBrains.Annotations;
using Sequencer.Domain;

namespace Sequencer.Midi
{
    public class TickCalculator : ITickCalculator
    {
        [NotNull] private readonly TimeSignature timeSignature;

        public TickCalculator()
        {
            // TODO: Inject
            timeSignature = new TimeSignature(4, 4);
        }

        public IPosition CalculatePositionFromTick(int tick, int quaterNoteResolution)
        {
            var beatsPerBar = timeSignature.BeatsPerBar;

            int ticksPerBeat = quaterNoteResolution / beatsPerBar;
            int currentBeat = (tick + ticksPerBeat) / ticksPerBeat;

            return Position.PositionFromBeat(currentBeat, timeSignature);
        }
    }
}