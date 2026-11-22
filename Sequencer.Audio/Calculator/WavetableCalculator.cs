using System;
using System.Collections.Generic;
using JetBrains.Annotations;

namespace Sequencer.Audio.Calculator
{
    /// <summary>
    /// Pre-computes one cycle of another waveform, then reads it back with linear interpolation.
    /// It holds no per-voice state, so one instance can be shared by every voice.
    /// </summary>
    public sealed class WavetableCalculator : IWaveformCalculator
    {
        private const int WavetableSize = 2048;

        [NotNull] private readonly Wavetable wavetable;

        public WavetableCalculator([NotNull] IWaveformCalculator calculator)
        {
            var samples = new List<float>(WavetableSize);

            for (var position = 0; position < WavetableSize; position++)
            {
                samples.Add(calculator.CalculateForPhase((double) position / WavetableSize));
            }

            wavetable = new Wavetable(samples);
        }

        public float CalculateForPhase(double phase)
        {
            double position = phase * wavetable.Size;
            var previousIndex = (int) Math.Floor(position);
            var fraction = (float) (position - previousIndex);

            // Wavetable wraps, so the sample after the last one is the first one.
            float previousSample = wavetable.SampleAtPosition(previousIndex);
            float nextSample = wavetable.SampleAtPosition(previousIndex + 1);

            return previousSample + (nextSample - previousSample) * fraction;
        }
    }
}
