using System;

namespace Sequencer.Audio.Calculator
{
    /// <summary>
    /// Builds a waveform by adding sine harmonics (additive synthesis).
    /// </summary>
    public abstract class ComplexWaveCalculator : IWaveformCalculator
    {
        private readonly int finalHarmonic;

        protected ComplexWaveCalculator(int finalHarmonic)
        {
            this.finalHarmonic = finalHarmonic;
        }

        public float CalculateForPhase(double phase)
        {
            double signal = 0;

            for (var harmonic = 1; harmonic <= finalHarmonic; harmonic++)
            {
                if (IsHarmonicIncluded(harmonic))
                {
                    signal += AmplitudeForHarmonic(harmonic) * Math.Sin(2 * Math.PI * harmonic * phase);
                }
            }

            return (float) signal;
        }

        protected abstract bool IsHarmonicIncluded(int harmonic);

        protected abstract float AmplitudeForHarmonic(int harmonic);
    }
}
