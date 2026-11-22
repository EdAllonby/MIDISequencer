using System;
using JetBrains.Annotations;
using Sequencer.Domain.Effects;

namespace Sequencer.Audio.Effects
{
    /// <summary>
    /// A feed-forward compressor: above the threshold, the level rises only 1/ratio as fast. The gain reduction
    /// follows the signal with the attack and release times, with a soft knee so it eases in. Makeup gain brings
    /// the quieter result back up.
    /// </summary>
    public sealed class CompressorProcessor : EffectProcessor
    {
        private const double KneeDecibels = 6;
        private double reductionDecibels;
        private Smoothed makeup;

        public CompressorProcessor([NotNull] Effect effect, int sampleRate) : base(effect, sampleRate)
        {
            makeup = new Smoothed(sampleRate);
        }

        /// <summary>
        /// The current gain reduction, in dB (0 or negative): for a meter.
        /// </summary>
        public double GainReductionDecibels => -reductionDecibels;

        public override void Process(Span<float> buffer)
        {
            double threshold = Effect["threshold"];
            double ratio = Effect["ratio"];
            double attack = GlideCoefficient(Effect["attack"] / 1000);
            double release = GlideCoefficient(Effect["release"] / 1000);
            makeup.Target = DecibelsToGain(Effect["makeup"]);
            double slope = 1 - 1 / ratio;

            for (var n = 0; n < buffer.Length; n++)
            {
                double level = GainToDecibels(Math.Abs(buffer[n]));
                double over = level - threshold;

                // How far to turn down, with a soft knee around the threshold.
                double wanted = over <= -KneeDecibels / 2 ? 0
                    : over >= KneeDecibels / 2 ? over * slope
                    : slope * (over + KneeDecibels / 2) * (over + KneeDecibels / 2) / (2 * KneeDecibels);

                reductionDecibels += (wanted - reductionDecibels) * (wanted > reductionDecibels ? attack : release);
                buffer[n] = (float) (buffer[n] * DecibelsToGain(-reductionDecibels) * makeup.Next());
            }
        }
    }
}
