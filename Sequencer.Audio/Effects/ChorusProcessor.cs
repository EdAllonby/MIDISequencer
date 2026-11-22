using System;
using JetBrains.Annotations;
using Sequencer.Domain.Effects;

namespace Sequencer.Audio.Effects
{
    /// <summary>
    /// A chorus: a copy of the sound, delayed by a time that slowly sways (7 ms, ± up to 5 ms), mixed back in.
    /// The changing delay detunes the copy slightly, which thickens the sound.
    /// </summary>
    public sealed class ChorusProcessor : EffectProcessor
    {
        private const double CentreSeconds = 0.007;
        private const double MaximumSwingSeconds = 0.005;

        [NotNull] private readonly float[] line;
        private int writePosition;
        private double lfoPhase;
        private Smoothed depth;
        private Smoothed mix;

        public ChorusProcessor([NotNull] Effect effect, int sampleRate) : base(effect, sampleRate)
        {
            line = new float[(int) ((CentreSeconds + MaximumSwingSeconds) * sampleRate) + 4];
            depth = new Smoothed(sampleRate, 0.05);
            mix = new Smoothed(sampleRate);
        }

        public override void Process(Span<float> buffer)
        {
            double increment = Effect["rate"] / SampleRate;
            depth.Target = Effect["depth"];
            mix.Target = Effect["mix"];

            for (var n = 0; n < buffer.Length; n++)
            {
                float dry = buffer[n];
                line[writePosition] = dry;

                double delaySamples = (CentreSeconds + MaximumSwingSeconds * depth.Next() * Math.Sin(2 * Math.PI * lfoPhase)) * SampleRate;
                double wet = DelayLine.ReadInterpolated(line, writePosition, delaySamples);
                lfoPhase += increment;
                lfoPhase -= Math.Floor(lfoPhase);
                writePosition = (writePosition + 1) % line.Length;

                // A linear blend: the copy is close to the dry sound, so an equal-power blend would swell 3 dB.
                double amount = mix.Next();
                buffer[n] = (float) (dry + (wet - dry) * amount);
            }
        }
    }

    internal static class DelayLine
    {
        /// <summary>
        /// Reads a circular buffer <paramref name="delaySamples"/> behind the sample just written, interpolating
        /// between samples (cubic Hermite), so a moving delay time doesn't crackle.
        /// </summary>
        public static double ReadInterpolated([NotNull] float[] line, int writePosition, double delaySamples)
        {
            int length = line.Length;
            delaySamples = Math.Clamp(delaySamples, 1, length - 3);
            double position = writePosition - delaySamples;
            int whole = (int) Math.Floor(position);
            double fraction = position - whole;

            float Sample(int index) => line[((index % length) + length) % length];

            double y0 = Sample(whole - 1), y1 = Sample(whole), y2 = Sample(whole + 1), y3 = Sample(whole + 2);
            double c1 = 0.5 * (y2 - y0);
            double c2 = y0 - 2.5 * y1 + 2 * y2 - 0.5 * y3;
            double c3 = 0.5 * (y3 - y0) + 1.5 * (y1 - y2);
            return ((c3 * fraction + c2) * fraction + c1) * fraction + y1;
        }
    }
}
