namespace Sequencer.Audio
{
    public readonly struct AudioOutputStatistics
    {
        public AudioOutputStatistics(long callbacks, long underflows, long clippedSamples, int smallestBuffer, int largestBuffer,
            float peakLevel, double outputLatencySeconds)
        {
            Callbacks = callbacks;
            Underflows = underflows;
            ClippedSamples = clippedSamples;
            SmallestBuffer = smallestBuffer;
            LargestBuffer = largestBuffer;
            PeakLevel = peakLevel;
            OutputLatencySeconds = outputLatencySeconds;
        }

        public long Callbacks { get; }

        /// <summary>
        /// Callbacks where PortAudio reported the device had run out of audio (an audible dropout).
        /// </summary>
        public long Underflows { get; }

        /// <summary>
        /// Samples beyond ±1, which the device clips.
        /// </summary>
        public long ClippedSamples { get; }

        public int SmallestBuffer { get; }

        public int LargestBuffer { get; }

        public float PeakLevel { get; }

        public double OutputLatencySeconds { get; }

        public override string ToString() =>
            $"callbacks={Callbacks} underflows={Underflows} clipped={ClippedSamples} peak={PeakLevel:F2} " +
            $"buffer={SmallestBuffer}-{LargestBuffer} frames requested latency={OutputLatencySeconds * 1000:F1} ms";
    }
}
