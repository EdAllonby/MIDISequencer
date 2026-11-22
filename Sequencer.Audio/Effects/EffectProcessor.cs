using System;
using JetBrains.Annotations;
using Sequencer.Domain.Effects;

namespace Sequencer.Audio.Effects
{
    /// <summary>
    /// The sound of one <see cref="Effect"/>: processes a track's audio in place, keeping whatever state it needs
    /// (envelopes, delay lines) between buffers. Parameters are read from the effect at each buffer and glided,
    /// so moving a slider while playing doesn't click.
    /// </summary>
    public abstract class EffectProcessor
    {
        protected EffectProcessor([NotNull] Effect effect, int sampleRate)
        {
            Effect = effect;
            SampleRate = sampleRate;
        }

        [NotNull]
        public Effect Effect { get; }

        protected int SampleRate { get; }

        public abstract void Process(Span<float> buffer);

        /// <summary>
        /// Creates the processor for an effect. <paramref name="beatsPerMinute"/> gives the tempo for timed effects.
        /// </summary>
        [NotNull]
        public static EffectProcessor Create([NotNull] Effect effect, int sampleRate, [NotNull] Func<double> beatsPerMinute)
        {
            switch (effect.Kind)
            {
                case EffectKind.Compressor:
                    return new CompressorProcessor(effect, sampleRate);
                case EffectKind.Equaliser:
                    return new EqualiserProcessor(effect, sampleRate);
                case EffectKind.Drive:
                    return new DriveProcessor(effect, sampleRate);
                case EffectKind.Chorus:
                    return new ChorusProcessor(effect, sampleRate);
                case EffectKind.Delay:
                    return new DelayProcessor(effect, sampleRate, beatsPerMinute);
                case EffectKind.Reverb:
                    return new ReverbProcessor(effect, sampleRate);
                default:
                    throw new ArgumentOutOfRangeException(nameof(effect), effect.Kind, null);
            }
        }

        protected static double DecibelsToGain(double decibels) => Math.Pow(10, decibels / 20);

        protected static double GainToDecibels(double gain) => 20 * Math.Log10(Math.Max(gain, 1e-9));

        /// <summary>
        /// The coefficient for a one-pole glide reaching about 63 % of the way in <paramref name="seconds"/>.
        /// </summary>
        protected double GlideCoefficient(double seconds) => 1 - Math.Exp(-1 / (Math.Max(seconds, 1e-6) * SampleRate));
    }

    /// <summary>
    /// A value that glides towards its target sample by sample, so parameter changes are smooth.
    /// </summary>
    internal struct Smoothed
    {
        private readonly double coefficient;
        private double current;
        private bool started;

        public Smoothed(int sampleRate, double seconds = 0.02)
        {
            coefficient = 1 - Math.Exp(-1 / (seconds * sampleRate));
            current = 0;
            started = false;
        }

        public double Target { get; set; }

        public double Next()
        {
            if (!started)
            {
                current = Target;
                started = true;
            }

            current += (Target - current) * coefficient;
            return current;
        }
    }

    /// <summary>
    /// A one-pole low-pass, for darkening echoes and taming the top of distortion.
    /// </summary>
    internal struct OnePoleLowPass
    {
        private double state;

        public double Process(double input, double coefficient)
        {
            state += (input - state) * coefficient;
            return state;
        }

        public static double CoefficientFor(double cutoff, int sampleRate) => 1 - Math.Exp(-2 * Math.PI * cutoff / sampleRate);
    }
}
