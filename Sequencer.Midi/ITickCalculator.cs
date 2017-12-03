using Sequencer.Domain;

namespace Sequencer.Midi
{
    public interface ITickCalculator
    {
        IPosition CalculatePositionFromTick(int tick, int quaterNoteResolution);
    }
}