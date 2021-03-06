﻿using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;

namespace Sequencer.Audio.Calculator
{
    public abstract class ComplexWaveCalculator : IWaveformCalculator
    {
        private readonly int finalHarmonic;
        [NotNull] private readonly SineWaveCalculator sineWaveCalculator = new SineWaveCalculator();

        protected ComplexWaveCalculator(int finalHarmonic)
        {
            this.finalHarmonic = finalHarmonic;
        }

        public float CalculateForSample(int sample, float frequency, int sampleRate)
        {
            IEnumerable<int> harmonicsToUse = Enumerable.Range(1, finalHarmonic).Where(IsHarmonicIncluded);

            IEnumerable<float> harmonicSignals = harmonicsToUse.Select(harmonic => CalculateSignalForHarmonic(sample, frequency, sampleRate, harmonic));

            return harmonicSignals.Sum();
        }

        private float CalculateSignalForHarmonic(int sample, float frequency, int sampleRate, int currentHarmonic)
        {
            float harmonicFrequency = frequency * currentHarmonic;
            float harmonicSignal = sineWaveCalculator.CalculateForSample(sample, harmonicFrequency, sampleRate);
            float harmonicAmplitude = AmplitudeForHarmonic(currentHarmonic);

            return harmonicAmplitude * harmonicSignal;
        }

        protected abstract bool IsHarmonicIncluded(int harmonic);

        protected abstract float AmplitudeForHarmonic(int harmonic);
    }
}