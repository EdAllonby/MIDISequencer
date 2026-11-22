using System;
using System.Linq;
using Moq;
using NAudio.Wave;
using NUnit.Framework;
using Sequencer.Domain.Settings;

namespace Sequencer.Audio.Tests
{
    [TestFixture]
    public class MasterBusTests
    {
        private const int SampleRate = 44100;

        /// <summary>
        /// A sine wave at a fixed amplitude (or an amplitude that changes over time).
        /// </summary>
        private sealed class TestSignal : ISampleProvider
        {
            private readonly Func<int, double> amplitudeAt;
            private int sample;

            public TestSignal(Func<int, double> amplitudeAt)
            {
                this.amplitudeAt = amplitudeAt;
            }

            public WaveFormat WaveFormat { get; } = WaveFormat.CreateIeeeFloatWaveFormat(SampleRate, 1);

            public int Read(Span<float> buffer)
            {
                for (var n = 0; n < buffer.Length; n++, sample++)
                {
                    buffer[n] = (float) (amplitudeAt(sample) * Math.Sin(2 * Math.PI * 220 * sample / SampleRate));
                }

                return buffer.Length;
            }
        }

        private static float[] Render(ISampleProvider provider, int sampleCount)
        {
            var output = new float[sampleCount];

            for (var offset = 0; offset < sampleCount; offset += 256)
            {
                provider.Read(output.AsSpan(offset, Math.Min(256, sampleCount - offset)));
            }

            return output;
        }

        [Test]
        public void Limiter_LeavesNormalLevelsUntouched()
        {
            float[] dry = Render(new TestSignal(_ => 0.6), SampleRate);
            var limiter = new LimiterSampleProvider(new TestSignal(_ => 0.6));
            float[] limited = Render(limiter, SampleRate);

            // Identical, just later by the look-ahead.
            int delay = limiter.LookaheadSamples;
            Assert.That(limited.Skip(delay), Is.EqualTo(dry.Take(dry.Length - delay)));
            Assert.That(limiter.Activity, Is.EqualTo(0));
            Assert.That(limiter.PeakInput, Is.EqualTo(0.6f).Within(0.001));
        }

        [Test]
        public void Limiter_KeepsLoudSignalsWithinFullScale()
        {
            // Like a dense chord: over three times full scale.
            float[] limited = Render(new LimiterSampleProvider(new TestSignal(_ => 3.4)), SampleRate);

            Assert.That(limited.Max(Math.Abs), Is.LessThanOrEqualTo(1f));
        }

        [Test]
        public void Limiter_CutsNoSharpCornersIntoTheWaveform_UnlikeClipping()
        {
            // Quiet, then very loud within 5 ms (like a chord's attack). Hard clipping flattens each peak with
            // sharp corners (heard as clicks and crackle); the limiter turns the gain down smoothly instead.
            const int rampStart = SampleRate / 2;
            const int rampLength = SampleRate / 200;
            Func<int, double> amplitude = n =>
            {
                double t = Math.Clamp((n - rampStart) / (double) rampLength, 0, 1);
                return 0.3 + 2.7 * t * t * (3 - 2 * t);
            };
            float[] limited = Render(new LimiterSampleProvider(new TestSignal(amplitude)), SampleRate);
            float[] clipped = Render(new TestSignal(amplitude), SampleRate).Select(x => Math.Clamp(x, -1f, 1f)).ToArray();

            Assert.That(SharpestCorner(limited), Is.LessThan(SharpestCorner(clipped) / 3));
        }

        /// <summary>
        /// The largest second difference, which spikes where the waveform changes direction abruptly.
        /// </summary>
        private static double SharpestCorner(float[] samples)
        {
            double sharpest = 0;

            for (var n = 2; n < samples.Length; n++)
            {
                sharpest = Math.Max(sharpest, Math.Abs(samples[n] - 2 * samples[n - 1] + samples[n - 2]));
            }

            return sharpest;
        }

        [Test]
        public void Limiter_TurnsTheGainDownBeforeAPeakArrives()
        {
            // Silence, then a sudden, very loud tone: with look-ahead the first loud samples out are already limited.
            var limiter = new LimiterSampleProvider(new TestSignal(n => n < SampleRate / 2 ? 0 : 3.0));
            float[] limited = Render(limiter, SampleRate);

            int arrival = SampleRate / 2 + limiter.LookaheadSamples;
            Assert.That(limited.Skip(arrival).Take(200).Max(Math.Abs), Is.LessThanOrEqualTo(LimiterSampleProvider.Threshold + 0.02f),
                "Already turned down when the peak comes out, not slammed down after it.");
        }

        [TestCase(0.0f)]
        [TestCase(0.5f)]
        [TestCase(-0.9f)]
        public void SoftCeiling_DoesNotChangeSamplesUpToTheThreshold(float sample)
        {
            Assert.That(LimiterSampleProvider.SoftCeiling(sample), Is.EqualTo(sample));
        }

        [Test]
        public void SoftCeiling_RisesSmoothlyAndNeverReachesBeyondFullScale()
        {
            float[] inputs = Enumerable.Range(0, 400).Select(i => i / 100f).ToArray();
            float[] outputs = inputs.Select(LimiterSampleProvider.SoftCeiling).ToArray();

            Assert.That(outputs, Is.Ordered, "Louder in, louder (or equal) out.");
            Assert.That(outputs.Max(), Is.LessThanOrEqualTo(1f));
        }

        [Test]
        public void MasterVolume_GlidesToANewValue()
        {
            var settings = new MixerSettings { MasterVolume = 1.0 };
            var master = new MasterVolumeSampleProvider(new TestSignal(_ => 1.0), settings);
            Render(master, 1000);

            settings.MasterVolume = 0.0;
            float[] afterChange = Render(master, SampleRate / 10);

            Assert.That(afterChange.Take(20).Max(Math.Abs), Is.GreaterThan(0.01f), "Not cut off instantly.");
            Assert.That(afterChange.Skip(SampleRate / 20).Max(Math.Abs), Is.LessThan(0.001f), "Silent after 50 ms.");
        }
    }
}
