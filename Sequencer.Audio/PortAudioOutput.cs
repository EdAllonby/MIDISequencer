using System;
using System.Runtime.InteropServices;
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
        private bool initialised;

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

            // Keep a reference to the delegate so it is not garbage collected while native code holds it.
            callback = OnAudioRequested;

            stream = new PortAudioSharp.Stream(null, outputParameters, sampleProvider.WaveFormat.SampleRate, 0,
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

            Marshal.Copy(buffer, 0, output, sampleCount);

            return StreamCallbackResult.Continue;
        }
    }
}
