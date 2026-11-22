namespace Sequencer.Audio.Calculator
{
    public interface IWaveformCalculator
    {
        /// <summary>
        /// Calculates the waveform's value at a point in its cycle.
        /// </summary>
        /// <param name="phase">Position within one cycle, from 0 (inclusive) to 1 (exclusive).</param>
        float CalculateForPhase(double phase);
    }
}
