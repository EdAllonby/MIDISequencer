using System;
using System.Diagnostics;
using System.Linq;
using NAudio.Wave;
using NUnit.Framework;
using Sequencer.Audio.Effects;
using Sequencer.Domain;
using Sequencer.Domain.Arrangement;
using Sequencer.Domain.Drums;
using Sequencer.Domain.Effects;
using Sequencer.Domain.Settings;
using Sequencer.Domain.Tunes;
using Sequencer.Midi;

namespace Sequencer.Audio.Tests
{
    [TestFixture]
    public class EffectProcessorTests
    {
        private const int SampleRate = 44100;

        private static float[] Sine(double frequency, double peak, int length) =>
            Enumerable.Range(0, length).Select(n => (float) (peak * Math.Sin(2 * Math.PI * frequency * n / SampleRate))).ToArray();

        private static float[] Run(Effect effect, float[] input, double bpm = 120)
        {
            EffectProcessor processor = EffectProcessor.Create(effect, SampleRate, () => bpm);
            float[] output = (float[]) input.Clone();

            // In realistic buffer sizes.
            for (var start = 0; start < output.Length; start += 512)
            {
                processor.Process(output.AsSpan(start, Math.Min(512, output.Length - start)));
            }

            return output;
        }

        private static double Rms(float[] samples, int from = 0) =>
            Math.Sqrt(samples.Skip(from).Select(s => (double) s * s).Average());

        private static double Decibels(double ratio) => 20 * Math.Log10(ratio);

        /// <summary>
        /// The gain at a frequency, measured on the second half (after the filter settles).
        /// </summary>
        private static double GainAt(Effect effect, double frequency)
        {
            float[] input = Sine(frequency, 0.25, SampleRate / 2);
            return Decibels(Rms(Run(effect, input), input.Length / 2) / Rms(input, input.Length / 2));
        }

        [Test]
        public void Compressor_LeavesQuietSoundsAlone_AndTurnsLoudOnesDownByTheRatio()
        {
            var compressor = new Effect(EffectKind.Compressor) { ["makeup"] = 0, ["threshold"] = -18, ["ratio"] = 4 };

            double quiet = Decibels(Rms(Run(compressor, Sine(200, 0.05, SampleRate)), SampleRate / 2) / Rms(Sine(200, 0.05, SampleRate)));
            float[] loudOut = Run(compressor, Sine(200, 0.5, SampleRate));
            double loudPeak = Decibels(loudOut.Skip(SampleRate / 2).Max(Math.Abs));

            TestContext.Out.WriteLine($"quiet {quiet:F2} dB, loud peak {loudPeak:F1} dBFS");
            Assert.That(quiet, Is.EqualTo(0).Within(0.1));

            // -6 dBFS peaks, 12 dB over: 4:1 lets 3 dB of that through, so about -15 dBFS.
            Assert.That(loudPeak, Is.EqualTo(-15).Within(2));
        }

        [Test]
        public void Compressor_Makeup_RaisesTheLevel()
        {
            var compressor = new Effect(EffectKind.Compressor) { ["makeup"] = 6, ["threshold"] = 0 };

            Assert.That(GainAt(compressor, 200), Is.EqualTo(6).Within(0.2));
        }

        [Test]
        public void Equaliser_WhenFlat_ChangesNothing()
        {
            var eq = new Effect(EffectKind.Equaliser);

            foreach (double frequency in new[] { 50.0, 1000, 12000 })
            {
                Assert.That(GainAt(eq, frequency), Is.EqualTo(0).Within(0.05), $"{frequency} Hz");
            }
        }

        [Test]
        public void Equaliser_BandsAffectTheirOwnRange()
        {
            var lows = new Effect(EffectKind.Equaliser) { ["low"] = 12 };
            var highs = new Effect(EffectKind.Equaliser) { ["high"] = -12 };
            var mids = new Effect(EffectKind.Equaliser) { ["mid"] = 9, ["midFrequency"] = 2000 };

            Assert.That(GainAt(lows, 40), Is.GreaterThan(10));
            Assert.That(GainAt(lows, 3000), Is.EqualTo(0).Within(0.5));
            Assert.That(GainAt(highs, 16000), Is.LessThan(-10));
            Assert.That(GainAt(highs, 300), Is.EqualTo(0).Within(0.5));
            Assert.That(GainAt(mids, 2000), Is.EqualTo(9).Within(0.3));
            Assert.That(GainAt(mids, 100), Is.EqualTo(0).Within(0.7));
        }

        [TestCase(0)]
        [TestCase(9)]
        [TestCase(24)]
        public void Drive_ChangesTheToneMoreThanTheVolume(double drive)
        {
            var effect = new Effect(EffectKind.Drive) { ["drive"] = drive, ["tone"] = 18000 };
            float[] input = Sine(220, 0.3, SampleRate / 2);

            double change = Decibels(Rms(Run(effect, input), 4410) / Rms(input, 4410));

            TestContext.Out.WriteLine($"drive {drive} dB → level {change:+0.0;-0.0} dB");
            Assert.That(change, Is.InRange(-3, 3));
        }

        [Test]
        public void Drive_AddsHarmonics_WithoutAliasing()
        {
            // A 3.5 kHz note driven hard: its harmonics above 22 kHz must not fold back into the audible range.
            const int fftSize = 16384;
            const int bin = 1310;
            var effect = new Effect(EffectKind.Drive) { ["drive"] = 24, ["tone"] = 18000 };
            float[] input = Sine(bin * (double) SampleRate / fftSize, 0.3, fftSize * 2);
            float[] output = Run(effect, input).Skip(fftSize).ToArray();

            var data = new NAudio.Dsp.Complex[fftSize];

            for (var i = 0; i < fftSize; i++)
            {
                data[i].X = output[i];
            }

            NAudio.Dsp.FastFourierTransform.FFT(true, 14, data);
            double Power(int i) => data[i].X * data[i].X + data[i].Y * data[i].Y;
            double harmonics = Enumerable.Range(1, fftSize / 2 - 1).Where(i => i % bin == 0).Sum(Power);
            double fundamental = Power(bin);
            double other = Enumerable.Range(1, fftSize / 2 - 1).Where(i => i % bin != 0).Sum(Power);

            double distortion = 10 * Math.Log10((harmonics - fundamental) / fundamental);
            double aliasing = 10 * Math.Log10(other / harmonics);
            TestContext.Out.WriteLine($"harmonics {distortion:F1} dB, aliasing {aliasing:F1} dB");
            Assert.That(distortion, Is.GreaterThan(-30), "Audibly driven.");
            Assert.That(aliasing, Is.LessThan(-40), "Little folded-back aliasing.");
        }

        [Test]
        public void Chorus_AtZeroMix_IsTheDrySound()
        {
            var chorus = new Effect(EffectKind.Chorus) { ["mix"] = 0 };
            float[] input = Sine(440, 0.3, 4096);

            Assert.That(Run(chorus, input), Is.EqualTo(input).Within(1e-6));
        }

        [Test]
        public void Chorus_KeepsTheLevelSteady()
        {
            var chorus = new Effect(EffectKind.Chorus);

            Assert.That(GainAt(chorus, 440), Is.InRange(-3, 1.5));
        }

        [Test]
        public void Delay_EchoesInTimeWithTheTempo()
        {
            // 1/8 dotted (3 sixteenths) at 100 BPM is 0.45 s.
            var delay = new Effect(EffectKind.Delay) { ["time"] = 2, ["feedback"] = 0.5, ["mix"] = 1, ["tone"] = 18000 };
            var input = new float[SampleRate * 2];
            input[0] = 1;

            float[] output = Run(delay, input, bpm: 100);
            int echo = (int) (0.45 * SampleRate);

            int Loudest(int from, int to) => Enumerable.Range(from, to - from).OrderByDescending(i => Math.Abs(output[i])).First();
            int first = Loudest(100, echo + 2000);
            int second = Loudest(echo + 2000, 2 * echo + 2000);

            Assert.That(first, Is.EqualTo(echo).Within(3));
            Assert.That(second, Is.EqualTo(2 * echo).Within(6));
            Assert.That(Math.Abs(output[second]), Is.LessThan(Math.Abs(output[first]) * 0.6), "Each repeat is quieter.");
        }

        [Test]
        public void Delay_AtFullFeedback_NeverRunsAway()
        {
            var delay = new Effect(EffectKind.Delay) { ["time"] = 0, ["feedback"] = 0.9, ["mix"] = 1 };

            float[] output = Run(delay, Sine(300, 0.5, SampleRate * 10));

            Assert.That(output.Max(Math.Abs), Is.LessThan(4));
        }

        [Test]
        public void Reverb_RingsOn_LongerForABiggerRoom_ThenDiesAway()
        {
            double TailLevel(double size, double afterSeconds)
            {
                var reverb = new Effect(EffectKind.Reverb) { ["size"] = size, ["mix"] = 1 };
                var input = new float[SampleRate * 8];

                for (var i = 0; i < 2205; i++)
                {
                    input[i] = (float) (0.5 * Math.Sin(2 * Math.PI * 300 * i / SampleRate));
                }

                float[] output = Run(reverb, input);
                int from = (int) (afterSeconds * SampleRate);
                return Rms(output.Skip(from).Take(4410).ToArray());
            }

            Assert.That(TailLevel(1, 1), Is.GreaterThan(TailLevel(0, 1) * 10), "A hall rings longer than a small room.");
            Assert.That(TailLevel(1, 7.5), Is.LessThan(TailLevel(1, 0.2) * 0.1), "Even the biggest room dies away.");
            Assert.That(TailLevel(0.5, 0.2), Is.GreaterThan(0.005));
        }

        private sealed class SineSource : ISampleProvider
        {
            private long n;

            public WaveFormat WaveFormat { get; } = WaveFormat.CreateIeeeFloatWaveFormat(SampleRate, 1);

            public int Read(Span<float> buffer)
            {
                for (var i = 0; i < buffer.Length; i++, n++)
                {
                    buffer[i] = (float) (0.3 * Math.Sin(2 * Math.PI * 220 * n / SampleRate));
                }

                return buffer.Length;
            }
        }

        private static float[] ReadBus(TrackBus bus, int buffers, Action<int> beforeBuffer)
        {
            var output = new float[buffers * 512];

            for (var b = 0; b < buffers; b++)
            {
                beforeBuffer(b);
                bus.Read(output.AsSpan(b * 512, 512));
            }

            return output;
        }

        private static double LargestStep(float[] samples, int from, int to) =>
            Enumerable.Range(from + 1, to - from - 1).Max(i => Math.Abs(samples[i] - samples[i - 1]));

        [Test]
        public void SwitchingEffectsWhilePlaying_DoesNotClick()
        {
            var track = new Track(TrackKind.Synth, "Synth", 0, 8);
            var bus = new TrackBus(track, SampleRate, () => 120);
            bus.AddVoice(new SineSource());
            var drive = new Effect(EffectKind.Drive) { ["drive"] = 24 };
            var eq = new Effect(EffectKind.Equaliser) { ["high"] = 12, ["low"] = -12 };

            float[] output = ReadBus(bus, 60, b =>
            {
                if (b == 10) track.AddEffect(drive);
                if (b == 20) track.AddEffect(eq);
                if (b == 30) drive.IsEnabled = false;
                if (b == 40) track.MoveEffect(eq, -1);
                if (b == 50) track.RemoveEffect(eq);
            });

            // A 220 Hz sine at 0.3 never moves more than about 0.01 per sample; driven and EQ'd, a little more.
            double steady = Math.Max(LargestStep(output, 0, 5000), LargestStep(output, 25 * 512, 29 * 512));
            double worst = LargestStep(output, 0, output.Length);
            TestContext.Out.WriteLine($"steady {steady:F4}, worst {worst:F4}");
            Assert.That(worst, Is.LessThan(steady * 1.5), "No sudden jump when the chain changes.");
        }

        [Test]
        public void ABypassedEffect_LeavesTheSoundUntouched()
        {
            var track = new Track(TrackKind.Synth, "Synth", 0, 8);
            var bus = new TrackBus(track, SampleRate, () => 120);
            bus.AddVoice(new SineSource());
            track.AddEffect(new Effect(EffectKind.Drive) { ["drive"] = 24, IsEnabled = false });

            float[] output = ReadBus(bus, 4, b => { });

            Assert.That(output, Is.EqualTo(Sine(220, 0.3, output.Length)).Within(1e-5));
        }

        [Test]
        public void ARetiredBus_FadesOutAndFinishes()
        {
            var bus = new TrackBus(new Track(TrackKind.Synth, "Synth", 0, 8), SampleRate, () => 120);
            bus.AddVoice(new SineSource());
            bus.Read(new float[512]);
            bus.Retire();

            var buffer = new float[512];
            int read = bus.Read(buffer);

            Assert.That(read, Is.EqualTo(512), "Still fading.");
            Assert.That(LargestStep(buffer, 0, 512), Is.LessThan(0.02));
            Assert.That(bus.Read(new float[512]), Is.EqualTo(0), "Finished, so the mixer drops it.");
        }

        [Test]
        public void ASongWithEveryEffectOnEveryTrack_RendersFarFasterThanRealTime()
        {
            Song song = Song.CreateDefault();
            ((NoteClip) song.Tracks[0].ClipAt(0)).SetTones(TuneLibrary.All.Single(t => t.Name == "Korobeiniki").ToTones(TimeSignature.FourFour, 96));
            ((DrumClip) song.Tracks[1].ClipAt(0)).Pattern.CopyFrom(DrumLoopLibrary.All.Single(l => l.Name == "Rock Beat").ToPattern());

            foreach (Track track in song.Tracks)
            {
                foreach (EffectKind kind in EffectCatalogue.All)
                {
                    track.AddEffect(new Effect(kind));
                }
            }

            var settings = new SequencerSettings();
            var renderer = new OfflineRenderer(new FrequencyCalculator(new PitchAndPositionCalculator(settings)), new TickCalculator(settings));
            renderer.Render(song, 120);
            Stopwatch timer = Stopwatch.StartNew();
            float[] samples = renderer.Render(song, 120);
            double speed = samples.Length / (double) SampleRate / timer.Elapsed.TotalSeconds;

            TestContext.Out.WriteLine($"{speed:F0}x real time; peak before limiter {renderer.PeakBeforeLimiter:F2}, over threshold {renderer.LimiterActivity:P2}");
            // Measured in a Debug build, where Drive's oversampling is slow; Release runs each effect 76-1200x real time.
            Assert.That(speed, Is.GreaterThan(3));
        }

        [Test]
        public void TypicalEffects_KeepTheMixWithinTheLimitersComfortZone()
        {
            Song song = Song.CreateDefault();
            ((NoteClip) song.Tracks[0].ClipAt(0)).SetTones(TuneLibrary.All.Single(t => t.Name == "Korobeiniki").ToTones(TimeSignature.FourFour, 96));
            song.Tracks[0].Synth.CopyFrom(SynthPresets.All.Single(p => p.Name == "Lead").CreatePatch());
            ((DrumClip) song.Tracks[1].ClipAt(0)).Pattern.CopyFrom(DrumLoopLibrary.All.Single(l => l.Name == "Rock Beat").ToPattern());
            song.Tracks[0].AddEffect(new Effect(EffectKind.Delay));
            song.Tracks[0].AddEffect(new Effect(EffectKind.Reverb));
            song.Tracks[1].AddEffect(new Effect(EffectKind.Compressor));
            song.Tracks[1].AddEffect(new Effect(EffectKind.Reverb));

            var settings = new SequencerSettings();
            var renderer = new OfflineRenderer(new FrequencyCalculator(new PitchAndPositionCalculator(settings)), new TickCalculator(settings));
            renderer.Render(song, 120);

            TestContext.Out.WriteLine($"peak before limiter {renderer.PeakBeforeLimiter:F2}, over threshold {renderer.LimiterActivity:P2}");
            Assert.That(renderer.LimiterActivity, Is.LessThan(0.002));
            Assert.That(renderer.PeakBeforeLimiter, Is.LessThan(1.25f));
        }
    }
}
