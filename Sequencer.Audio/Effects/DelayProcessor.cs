using System;
using JetBrains.Annotations;
using Sequencer.Domain.Effects;

namespace Sequencer.Audio.Effects
{
    /// <summary>
    /// An echo in time with the tempo (a sixteenth up to a half note). Each repeat is fed back a little quieter
    /// and darker, like a tape echo. Changing the time or tempo glides the delay rather than jumping.
    /// </summary>
    public sealed class DelayProcessor : EffectProcessor
    {
        private const double MinimumBeatsPerMinute = 30;

        [NotNull] private readonly Func<double> beatsPerMinute;
        [NotNull] private readonly float[] line;
        private int writePosition;
        private Smoothed delaySamples;
        private Smoothed feedback;
        private Smoothed mix;
        private OnePoleLowPass tone;

        public DelayProcessor([NotNull] Effect effect, int sampleRate, [NotNull] Func<double> beatsPerMinute) : base(effect, sampleRate)
        {
            this.beatsPerMinute = beatsPerMinute;

            // Long enough for the longest division at the slowest tempo.
            line = new float[(int) (EffectCatalogue.DelaySixteenths[EffectCatalogue.DelaySixteenths.Count - 1] / 4 * 60 / MinimumBeatsPerMinute * sampleRate) + 8];
            delaySamples = new Smoothed(sampleRate, 0.15);
            feedback = new Smoothed(sampleRate);
            mix = new Smoothed(sampleRate);
        }

        /// <summary>
        /// The echo time now, in seconds.
        /// </summary>
        public double DelaySeconds
        {
            get
            {
                double sixteenths = EffectCatalogue.DelaySixteenths[(int) Effect["time"]];
                return sixteenths / 4 * 60 / Math.Max(MinimumBeatsPerMinute, beatsPerMinute());
            }
        }

        public override void Process(Span<float> buffer)
        {
            delaySamples.Target = DelaySeconds * SampleRate;
            feedback.Target = Effect["feedback"];
            mix.Target = Effect["mix"];
            double toneCoefficient = OnePoleLowPass.CoefficientFor(Effect["tone"], SampleRate);

            for (var n = 0; n < buffer.Length; n++)
            {
                float dry = buffer[n];
                double echo = DelayLine.ReadInterpolated(line, writePosition, delaySamples.Next());
                double darkened = tone.Process(echo, toneCoefficient);

                // Feedback gently saturated, so high feedback can't run away.
                line[writePosition] = (float) Math.Tanh(dry + darkened * feedback.Next());
                writePosition = (writePosition + 1) % line.Length;

                // Echoes are added on top: the dry sound stays at full level.
                buffer[n] = (float) (dry + darkened * mix.Next());
            }
        }
    }
}
