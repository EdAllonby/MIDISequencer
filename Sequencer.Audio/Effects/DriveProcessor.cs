using System;
using JetBrains.Annotations;
using Sequencer.Domain.Effects;

namespace Sequencer.Audio.Effects
{
    /// <summary>
    /// Saturation: the signal is boosted into a smooth tanh curve, scaled so more drive means more harmonics
    /// rather than more volume. The new harmonics would fold back down as aliasing, so the curve runs at four
    /// times the sample rate between steep (8th-order) filters, and uses antiderivative anti-aliasing, which
    /// suppresses the harmonics that reach even past the higher rate. A tone control rounds off the top.
    /// </summary>
    public sealed class DriveProcessor : EffectProcessor
    {
        private const int Oversampling = 4;

        /// <summary>
        /// A typical peak on a track. The curve is scaled so this level comes out where it went in.
        /// </summary>
        private const double ReferenceLevel = 0.3;

        // Butterworth 8th order as four sections.
        private static readonly double[] SectionQs = { 0.5098, 0.6013, 0.9000, 2.5629 };

        [NotNull] private readonly Biquad[] upFilter;
        [NotNull] private readonly Biquad[] downFilter;
        [NotNull] private readonly Biquad[] dryUpFilter;
        [NotNull] private readonly Biquad[] dryDownFilter;
        private Smoothed drive;
        private Smoothed mix;
        private OnePoleLowPass tone;
        private double previousInput;
        private double previousShaperInput;

        public DriveProcessor([NotNull] Effect effect, int sampleRate) : base(effect, sampleRate)
        {
            drive = new Smoothed(sampleRate);
            mix = new Smoothed(sampleRate);
            upFilter = AntiAliasingFilter(sampleRate);
            downFilter = AntiAliasingFilter(sampleRate);
            dryUpFilter = AntiAliasingFilter(sampleRate);
            dryDownFilter = AntiAliasingFilter(sampleRate);
        }

        public override void Process(Span<float> buffer)
        {
            drive.Target = DecibelsToGain(Effect["drive"]);
            mix.Target = Effect["mix"];
            double toneCoefficient = OnePoleLowPass.CoefficientFor(Effect["tone"], SampleRate);

            for (var n = 0; n < buffer.Length; n++)
            {
                double gain = drive.Next();
                double scale = ReferenceLevel / Math.Tanh(gain * ReferenceLevel);
                double input = buffer[n];
                double wet = 0;
                double dry = 0;

                for (var k = 0; k < Oversampling; k++)
                {
                    // Linear interpolation between samples, then filtered: a simple, clean upsampler.
                    double stepped = previousInput + (input - previousInput) * (k + 1) / Oversampling;
                    double upsampled = Run(upFilter, stepped);
                    wet = Run(downFilter, Shape(upsampled * gain) * scale);

                    // The dry sound goes through the same filters, so blending the two can't comb-filter.
                    dry = Run(dryDownFilter, Run(dryUpFilter, stepped));
                }

                previousInput = input;
                double toned = tone.Process(wet, toneCoefficient);
                buffer[n] = (float) (dry + (toned - dry) * mix.Next());
            }
        }

        /// <summary>
        /// tanh with first-order antiderivative anti-aliasing: the average of tanh between this input and the
        /// last, from its antiderivative ln cosh.
        /// </summary>
        private double Shape(double x)
        {
            double previous = previousShaperInput;
            previousShaperInput = x;
            double difference = x - previous;

            return Math.Abs(difference) < 1e-5
                ? Math.Tanh((x + previous) / 2)
                : (LogCosh(x) - LogCosh(previous)) / difference;
        }

        private static double LogCosh(double x)
        {
            double magnitude = Math.Abs(x);
            return magnitude + Math.Log(1 + Math.Exp(-2 * magnitude)) - Math.Log(2);
        }

        [NotNull]
        private static Biquad[] AntiAliasingFilter(int sampleRate)
        {
            return Array.ConvertAll(SectionQs, q => Biquad.LowPass(sampleRate * Oversampling, sampleRate * 0.42, q));
        }

        private static double Run([NotNull] Biquad[] sections, double sample)
        {
            for (var i = 0; i < sections.Length; i++)
            {
                sample = sections[i].Process(sample);
            }

            return sample;
        }
    }
}
