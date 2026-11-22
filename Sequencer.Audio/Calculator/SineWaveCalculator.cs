using System;

namespace Sequencer.Audio.Calculator
{
    public sealed class SineWaveCalculator : IWaveformCalculator
    {
        public float CalculateForPhase(double phase)
        {
            return (float) Math.Sin(2 * Math.PI * phase);
        }
    }
}
