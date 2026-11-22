using System;
using JetBrains.Annotations;
using NAudio.Wave;

namespace Sequencer.Audio
{
    /// <summary>
    /// Keeps the mix within ±1 so it never hard-clips. Overlapping notes add up (a chord of sawtooth notes can
    /// peak at several times full scale), and the device cuts off anything beyond ±1, which sounds like clicks.
    /// </summary>
    /// <remarks>
    /// An envelope follower tracks the signal level, rising fast (attack) and falling slowly (release). Above
    /// the threshold the gain is reduced so the level sits at the threshold; below it the signal passes
    /// unchanged. Peaks faster than the attack are caught by a soft ceiling that approaches, but never
    /// exceeds, 1.
    /// </remarks>
    public sealed class LimiterSampleProvider : ISampleProvider
    {
        public const float Threshold = 0.9f;
        private const double AttackSeconds = 0.0005;
        private const double ReleaseSeconds = 0.15;

        [NotNull] private readonly ISampleProvider source;
        private readonly float attackCoefficient;
        private readonly float releaseCoefficient;
        private float envelope;

        public LimiterSampleProvider([NotNull] ISampleProvider source)
        {
            this.source = source;
            int sampleRate = source.WaveFormat.SampleRate;
            attackCoefficient = (float) Math.Exp(-1.0 / (AttackSeconds * sampleRate));
            releaseCoefficient = (float) Math.Exp(-1.0 / (ReleaseSeconds * sampleRate));
        }

        public WaveFormat WaveFormat => source.WaveFormat;

        public int Read(Span<float> buffer)
        {
            int read = source.Read(buffer);

            for (var n = 0; n < read; n++)
            {
                float sample = buffer[n];
                float level = Math.Abs(sample);
                float coefficient = level > envelope ? attackCoefficient : releaseCoefficient;
                envelope = coefficient * envelope + (1 - coefficient) * level;

                float gain = envelope > Threshold ? Threshold / envelope : 1f;
                buffer[n] = SoftCeiling(sample * gain);
            }

            return read;
        }

        /// <summary>
        /// Unchanged up to the threshold, then curves smoothly towards ±1 (with matching slope at the
        /// threshold, so there is no kink to hear).
        /// </summary>
        public static float SoftCeiling(float sample)
        {
            float level = Math.Abs(sample);

            if (level <= Threshold)
            {
                return sample;
            }

            const float headroom = 1f - Threshold;
            float shaped = Threshold + headroom * (float) Math.Tanh((level - Threshold) / headroom);
            return Math.Sign(sample) * shaped;
        }
    }
}
