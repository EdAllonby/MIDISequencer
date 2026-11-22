using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Sequencer.Audio.Calculator;
using Sequencer.Domain;
using Sequencer.Domain.Arrangement;
using Sequencer.Domain.Drums;
using Sequencer.Domain.Settings;
using Sequencer.Domain.Tunes;
using Sequencer.Midi;

namespace Sequencer.Audio.Tests
{
    /// <summary>
    /// Guards against the audible artifacts found by measuring rendered songs: a mix too hot for the limiter,
    /// drum hits cut off or started with a click, and aliasing on high notes.
    /// </summary>
    [TestFixture]
    public class ArtifactTests
    {
        private const int SampleRate = 44100;
        private const int FftSize = 16384;

        /// <summary>
        /// The energy of a waveform at non-harmonic frequencies, relative to its harmonics, in dB. The note is
        /// placed exactly on an FFT bin, so the harmonics land on bins too and everything else is error.
        /// </summary>
        private static double InharmonicDecibels(IWaveformCalculator waveform, int bin)
        {
            double frequency = bin * (double) SampleRate / FftSize;
            var data = new NAudio.Dsp.Complex[FftSize];
            double phase = 0;

            for (var i = 0; i < FftSize; i++)
            {
                data[i].X = waveform.CalculateForPhase(phase, frequency);
                phase += frequency / SampleRate;
                phase -= Math.Floor(phase);
            }

            NAudio.Dsp.FastFourierTransform.FFT(true, 14, data);
            double harmonic = 0, other = 0;

            for (var i = 1; i < FftSize / 2; i++)
            {
                double power = data[i].X * data[i].X + data[i].Y * data[i].Y;

                if (i % bin == 0)
                {
                    harmonic += power;
                }
                else
                {
                    other += power;
                }
            }

            return 10 * Math.Log10(other / harmonic);
        }

        [TestCase(Waveform.Sawtooth, 655)] // ~1.76 kHz
        [TestCase(Waveform.Sawtooth, 1310)] // ~3.5 kHz
        [TestCase(Waveform.Square, 1310)]
        [TestCase(Waveform.Triangle, 1310)]
        public void HighNotes_DoNotAlias(Waveform waveform, int bin)
        {
            Assert.That(InharmonicDecibels(BandLimitedWavetable.Standard[waveform], bin), Is.LessThan(-60));
        }

        [Test]
        public void AFixedHarmonicWavetable_DoesAlias_WhichIsWhyTheyAreBandLimited()
        {
            Assert.That(InharmonicDecibels(new WavetableCalculator(new SawtoothWaveCalculator(19)), 1310), Is.GreaterThan(-30));
        }

        [Test]
        public void LowNotes_KeepMoreHarmonicsThanHighOnes()
        {
            IWaveformCalculator saw = BandLimitedWavetable.Standard[Waveform.Sawtooth];

            // The steepest drop in one cycle reflects how many harmonics are present.
            double Steepest(double frequency) => Enumerable.Range(0, 2048)
                .Select(i => Math.Abs(saw.CalculateForPhase((i + 1) / 2048.0, frequency) - saw.CalculateForPhase(i / 2048.0, frequency))).Max();

            Assert.That(Steepest(55), Is.GreaterThan(Steepest(1760) * 4));
        }

        private static IEnumerable<DrumSound> Sounds => DrumPattern.AllSounds;

        [TestCaseSource(nameof(Sounds))]
        public void DrumHits_FadeToSilenceBeforeTheyEnd(DrumSound sound)
        {
            var voice = new DrumVoice(sound, 127, SampleRate);
            int length = (int) (DrumVoice.LengthOf(sound) * SampleRate);
            var samples = new float[length + 256];
            voice.Read(samples);

            Assert.That(samples.Skip(length - 88).Take(88).Max(Math.Abs), Is.LessThan(0.001f), "No click where the hit stops.");
        }

        [TestCaseSource(nameof(Sounds))]
        public void DrumHits_StartFromSilence(DrumSound sound)
        {
            var samples = new float[64];
            new DrumVoice(sound, 127, SampleRate).Read(samples);

            Assert.That(Math.Abs(samples[0]), Is.LessThan(0.01f), "No click where the hit starts.");
        }

        private static IEnumerable<TestCaseData> Songs()
        {
            yield return new TestCaseData("Korobeiniki", "Init", "Disco", null).SetName("Lead and drums");
            yield return new TestCaseData("Korobeiniki", "Acid Bass", "Boom Bap", null).SetName("Acid bass and drums");
            yield return new TestCaseData("Korobeiniki", "Lead", "Rock Beat", "Canon in D").SetName("Lead, pad and drums");
        }

        [TestCaseSource(nameof(Songs))]
        public void TypicalSongs_LeaveTheLimiterAlmostIdle(string tune, string preset, string loop, string padTune)
        {
            Song song = Song.CreateDefault();
            ((NoteClip) song.Tracks[0].ClipAt(0)).SetTones(TuneLibrary.All.Single(t => t.Name == tune).ToTones(TimeSignature.FourFour, 96));
            song.Tracks[0].Synth.CopyFrom(SynthPresets.All.Single(p => p.Name == preset).CreatePatch());
            ((DrumClip) song.Tracks[1].ClipAt(0)).Pattern.CopyFrom(DrumLoopLibrary.All.Single(l => l.Name == loop).ToPattern());

            if (padTune != null)
            {
                Track pad = song.AddTrack(TrackKind.Synth);
                var clip = (NoteClip) song.CreateClip(pad);
                song.Place(pad, 0, clip);
                clip.SetTones(TuneLibrary.All.Single(t => t.Name == padTune).ToTones(TimeSignature.FourFour, 96));
                pad.Synth.CopyFrom(SynthPresets.All.Single(p => p.Name == "Warm Pad").CreatePatch());
            }

            var settings = new SequencerSettings();
            var renderer = new OfflineRenderer(new FrequencyCalculator(new PitchAndPositionCalculator(settings)), new TickCalculator(settings));

            float[] samples = renderer.Render(song, 120);

            Assert.That(samples.Max(Math.Abs), Is.LessThanOrEqualTo(1f));
            Assert.That(samples.Max(Math.Abs), Is.GreaterThan(0.2f), "Not so quiet that it's useless.");
            TestContext.Out.WriteLine($"peak before limiter {renderer.PeakBeforeLimiter:F2}, over threshold {renderer.LimiterActivity:P2}");
            Assert.That(renderer.LimiterActivity, Is.LessThan(0.002), "Peaks over the threshold are rare: the limiter is a safety net.");
            Assert.That(renderer.PeakBeforeLimiter, Is.LessThan(1.25f), "At most a couple of dB of limiting on the loudest moments.");
        }
    }
}
