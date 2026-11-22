using System;
using System.Runtime.InteropServices;
using System.Threading;
using JetBrains.Annotations;
using NAudio.Wave;
using PortAudioSharp;

namespace Sequencer.Audio
{
    /// <summary>
    /// Plays audio through PortAudio, which uses CoreAudio on macOS, WASAPI/MME on Windows and ALSA on Linux.
    /// </summary>
    public sealed class PortAudioOutput : IAudioOutput
    {
        [CanBeNull] private PortAudioSharp.Stream stream;
        [CanBeNull] private ISampleProvider source;
        [CanBeNull] private Stream.Callback callback;
        [NotNull] private float[] buffer = new float[8192];
        private readonly uint framesPerBuffer;
        private bool initialised;

        // Diagnostics, written only by the audio thread.
        private long callbackCount;
        private long underflowCount;
        private long clippedSampleCount;
        private int smallestBuffer = int.MaxValue;
        private int largestBuffer;
        private float peakLevel;
        private double requestedLatencySeconds;

        /// <param name="framesPerBuffer">Frames per audio callback, or 0 to let PortAudio choose.</param>
        public PortAudioOutput(uint framesPerBuffer = 0)
        {
            this.framesPerBuffer = framesPerBuffer;
        }

        /// <summary>
        /// Counts of underruns (the device ran out of audio) and clipped samples (beyond ±1) since playback began.
        /// </summary>
        public AudioOutputStatistics Statistics => new AudioOutputStatistics(
            Interlocked.Read(ref callbackCount), Interlocked.Read(ref underflowCount), Interlocked.Read(ref clippedSampleCount),
            smallestBuffer == int.MaxValue ? 0 : smallestBuffer, largestBuffer, peakLevel,
            requestedLatencySeconds);

        public void Play(ISampleProvider sampleProvider)
        {
            if (stream != null)
            {
                throw new InvalidOperationException("Audio output is already playing.");
            }

            source = sampleProvider;

            PortAudio.Initialize();
            initialised = true;

            int device = PortAudio.DefaultOutputDevice;

            if (device == PortAudio.NoDevice)
            {
                throw new InvalidOperationException("No audio output device found.");
            }

            DeviceInfo deviceInfo = PortAudio.GetDeviceInfo(device);

            var outputParameters = new StreamParameters
            {
                device = device,
                channelCount = sampleProvider.WaveFormat.Channels,
                sampleFormat = SampleFormat.Float32,
                suggestedLatency = deviceInfo.defaultLowOutputLatency,
                hostApiSpecificStreamInfo = IntPtr.Zero
            };

            requestedLatencySeconds = outputParameters.suggestedLatency;

            // Keep a reference to the delegate so it is not garbage collected while native code holds it.
            callback = OnAudioRequested;

            stream = new PortAudioSharp.Stream(null, outputParameters, sampleProvider.WaveFormat.SampleRate, framesPerBuffer,
                StreamFlags.ClipOff, callback, IntPtr.Zero);
            stream.Start();
        }

        public void Dispose()
        {
            // The stream must be closed before PortAudio is terminated.
            if (stream != null)
            {
                stream.Stop();
                stream.Dispose();
                stream = null;
            }

            if (initialised)
            {
                PortAudio.Terminate();
                initialised = false;
            }
        }

        // Runs on the real-time audio thread: no locks of our own, no logging, and no allocation except
        // the rare buffer growth below.
        private StreamCallbackResult OnAudioRequested(IntPtr input, IntPtr output, uint frameCount,
            ref StreamCallbackTimeInfo timeInfo, StreamCallbackFlags statusFlags, IntPtr userData)
        {
            ISampleProvider currentSource = source;
            int sampleCount = (int) frameCount * (currentSource?.WaveFormat.Channels ?? 1);

            if (buffer.Length < sampleCount)
            {
                buffer = new float[sampleCount];
            }

            Span<float> samples = buffer.AsSpan(0, sampleCount);

            try
            {
                int read = currentSource?.Read(samples) ?? 0;
                samples.Slice(read).Clear();
            }
            catch (Exception)
            {
                // An exception must never escape into native code; play silence instead.
                samples.Clear();
            }

            RecordStatistics(samples, statusFlags, (int) frameCount);
            Marshal.Copy(buffer, 0, output, sampleCount);

            return StreamCallbackResult.Continue;
        }

        private void RecordStatistics(ReadOnlySpan<float> samples, StreamCallbackFlags statusFlags, int frameCount)
        {
            Interlocked.Increment(ref callbackCount);

            if ((statusFlags & StreamCallbackFlags.OutputUnderflow) != 0)
            {
                Interlocked.Increment(ref underflowCount);
            }

            smallestBuffer = Math.Min(smallestBuffer, frameCount);
            largestBuffer = Math.Max(largestBuffer, frameCount);

            foreach (float sample in samples)
            {
                float level = Math.Abs(sample);

                if (level > peakLevel)
                {
                    peakLevel = level;
                }

                if (level > 1f)
                {
                    Interlocked.Increment(ref clippedSampleCount);
                }
            }
        }
    }
}
