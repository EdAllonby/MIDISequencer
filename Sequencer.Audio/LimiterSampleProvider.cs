using System;
using JetBrains.Annotations;
using NAudio.Wave;

namespace Sequencer.Audio
{
    /// <summary>
    /// Keeps the mix within ±1 so it never hard-clips, without audible distortion. It looks ahead: the signal is
    /// delayed by 2 ms while the limiter watches what's coming, so it can turn the gain down smoothly before a
    /// peak arrives rather than slamming it down when the peak is already playing (which distorts kicks and bass,
    /// because the gain then changes within a single cycle).
    /// </summary>
    /// <remarks>
    /// For each incoming sample it works out the gain that would keep it at the threshold. The smallest such
    /// gain anywhere in the look-ahead window is the target; the gain glides down to it within the window and
    /// recovers slowly (150 ms) once the peaks pass. A soft ceiling just below 1 catches anything left over.
    /// </remarks>
    public sealed class LimiterSampleProvider : ISampleProvider
    {
        public const float Threshold = 0.9f;
        public const double LookaheadSeconds = 0.002;
        private const double ReleaseSeconds = 0.15;

        [NotNull] private readonly ISampleProvider source;
        [NotNull] private readonly float[] delay;
        [NotNull] private readonly float[] windowGains;
        [NotNull] private readonly int[] windowIndices;
        private readonly float attackCoefficient;
        private readonly float releaseCoefficient;
        private int delayPosition;
        private long sampleIndex;

        // A monotonic queue of (index, required gain), giving the window's minimum in constant time.
        private int windowHead;
        private int windowCount;
        private float gain = 1f;

        public LimiterSampleProvider([NotNull] ISampleProvider source)
        {
            this.source = source;
            int sampleRate = source.WaveFormat.SampleRate;
            LookaheadSamples = Math.Max(1, (int) Math.Round(LookaheadSeconds * sampleRate));
            delay = new float[LookaheadSamples];
            windowGains = new float[LookaheadSamples + 1];
            windowIndices = new int[LookaheadSamples + 1];

            // Reach 98 % of the target within the look-ahead window.
            attackCoefficient = (float) (1 - Math.Exp(-4.0 / LookaheadSamples));
            releaseCoefficient = (float) (1 - Math.Exp(-1.0 / (ReleaseSeconds * sampleRate)));
        }

        /// <summary>
        /// How many samples the output lags the input.
        /// </summary>
        public int LookaheadSamples { get; }

        /// <summary>
        /// The fraction of samples so far that came in above the threshold, so needed limiting. In a well-balanced
        /// mix it's close to 0; if it's often high, the mix is too hot.
        /// </summary>
        public double Activity => sampleIndex == 0 ? 0 : (double) samplesOverThreshold / sampleIndex;

        /// <summary>
        /// The loudest sample that has come in, before limiting.
        /// </summary>
        public float PeakInput { get; private set; }

        private long samplesOverThreshold;

        public WaveFormat WaveFormat => source.WaveFormat;

        public int Read(Span<float> buffer)
        {
            int read = source.Read(buffer);

            for (var n = 0; n < read; n++)
            {
                float incoming = buffer[n];
                float level = Math.Abs(incoming);
                PushRequiredGain(level > Threshold ? Threshold / level : 1f);

                if (level > Threshold)
                {
                    samplesOverThreshold++;
                }

                PeakInput = Math.Max(PeakInput, level);

                float target = windowGains[windowHead];
                gain += (target - gain) * (target < gain ? attackCoefficient : releaseCoefficient);

                float delayed = delay[delayPosition];
                delay[delayPosition] = incoming;
                delayPosition = (delayPosition + 1) % delay.Length;

                buffer[n] = SoftCeiling(delayed * gain);
                sampleIndex++;
            }

            return read;
        }

        private void PushRequiredGain(float required)
        {
            int size = windowGains.Length;

            // Drop entries no smaller than the new one (they can never be the minimum again)...
            while (windowCount > 0)
            {
                int last = (windowHead + windowCount - 1) % size;

                if (windowGains[last] < required)
                {
                    break;
                }

                windowCount--;
            }

            int slot = (windowHead + windowCount) % size;
            windowGains[slot] = required;
            windowIndices[slot] = (int) (sampleIndex % int.MaxValue);
            windowCount++;

            // ...and entries that have left the window.
            while (windowCount > 1 && (int) (sampleIndex % int.MaxValue) - windowIndices[windowHead] > LookaheadSamples)
            {
                windowHead = (windowHead + 1) % size;
                windowCount--;
            }
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
