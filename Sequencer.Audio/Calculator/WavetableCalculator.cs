using System;
using System.Collections.Generic;
using JetBrains.Annotations;

namespace Sequencer.Audio.Calculator
{
    public sealed class WavetableCalculator : IWaveformCalculator
    {
        [NotNull] private readonly Wavetable wavetable;

        private float currentWavetablePosition;

        public WavetableCalculator([NotNull] IWaveformCalculator calculator)
        {
            var wavetableSize = 1024;

            var wavetableSamples = new List<float>();

            for (var position = 0; position < wavetableSize; position++)
            {
                float sample = calculator.CalculateForSample(position, wavetableSize, wavetableSize * wavetableSize);
                wavetableSamples.Add(sample);
            }

            wavetable = new Wavetable(wavetableSamples);
        }

        public float CalculateForSample(int sample, float frequency, int sampleRate)
        {
            float interpolatedSample = InterpolateCurrentPosition();

            float step = wavetable.Size * frequency / sampleRate;

            currentWavetablePosition = (currentWavetablePosition + step) % (wavetable.Size - 1);

            return interpolatedSample;
        }

        private float InterpolateCurrentPosition()
        {
            float previousSample = wavetable.SampleAtPosition((int) Math.Floor(currentWavetablePosition));
            float nextSample = wavetable.SampleAtPosition((int) Math.Ceiling(currentWavetablePosition));

            return LinearInterpolate(previousSample, nextSample);
        }

        private float LinearInterpolate(float previousSample, float nextSample)
        {
            float positionFraction = GetFraction(currentWavetablePosition);

            return (nextSample - previousSample) * positionFraction + previousSample;
        }

        private static float GetFraction(float value)
        {
            return (float) (value - Math.Truncate(value));
        }
    }
}