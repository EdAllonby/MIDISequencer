using System;
using JetBrains.Annotations;
using Sequencer.Domain.Effects;

namespace Sequencer.Audio.Effects
{
    /// <summary>
    /// A three-band EQ: a low shelf at 120 Hz, a sweepable mid bell, and a high shelf at 8 kHz.
    /// </summary>
    public sealed class EqualiserProcessor : EffectProcessor
    {
        private const double LowFrequency = 120;
        private const double HighFrequency = 8000;
        private const double MidOctaves = 1.5;

        private Biquad low;
        private Biquad mid;
        private Biquad high;
        private double lastLow = double.NaN;
        private double lastMid = double.NaN;
        private double lastMidFrequency = double.NaN;
        private double lastHigh = double.NaN;

        public EqualiserProcessor([NotNull] Effect effect, int sampleRate) : base(effect, sampleRate)
        {
        }

        public override void Process(Span<float> buffer)
        {
            // Always filtered, even when flat (where it's transparent), so returning a band to 0 can't jump.
            UpdateFilters();

            for (var n = 0; n < buffer.Length; n++)
            {
                buffer[n] = (float) high.Process(mid.Process(low.Process(buffer[n])));
            }
        }

        private void UpdateFilters()
        {
            double lowGain = Effect["low"], midGain = Effect["mid"], midFrequency = Effect["midFrequency"], highGain = Effect["high"];

            // Recalculating coefficients keeps the filters' state, so the change is smooth.
            if (lowGain != lastLow)
            {
                low.SetLowShelf(SampleRate, LowFrequency, lowGain);
                lastLow = lowGain;
            }

            if (midGain != lastMid || midFrequency != lastMidFrequency)
            {
                mid.SetPeaking(SampleRate, midFrequency, MidOctaves, midGain);
                lastMid = midGain;
                lastMidFrequency = midFrequency;
            }

            if (highGain != lastHigh)
            {
                high.SetHighShelf(SampleRate, HighFrequency, highGain);
                lastHigh = highGain;
            }
        }
    }
}
