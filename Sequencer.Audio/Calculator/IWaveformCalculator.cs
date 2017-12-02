namespace Sequencer.Audio.Calculator
{
    public interface IWaveformCalculator
    {
        float CalculateForSample(int sample, float frequency, int sampleRate);
    }
}
