using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Sequencer.Audio.Calculator;
using Sequencer.Domain.Settings;

namespace Sequencer.Audio.Tests
{
    [TestFixture]
    public class SubtractiveVoiceTests
    {
        private const int SampleRate = 44100;

        private static readonly Dictionary<Waveform, IWaveformCalculator> Waveforms = new Dictionary<Waveform, IWaveformCalculator>
        {
            { Waveform.Sawtooth, new WavetableCalculator(new SawtoothWaveCalculator(19)) },
            { Waveform.Square, new WavetableCalculator(new SquareWaveCalculator(19)) },
            { Waveform.Triangle, new WavetableCalculator(new TriangleWaveCalculator(19)) },
            { Waveform.Sine, new SineWaveCalculator() }
        };

        private static IEnumerable<SynthPreset> Presets => SynthPresets.All;

        private static SynthSettings Patch(Action<SynthSettings> configure = null)
        {
            // Instant attack and full sustain, so measurements see the filter and oscillators, not the envelope.
            var patch = new SynthSettings { AttackSeconds = 0.001, DecaySeconds = 0.001, SustainLevel = 1 };
            configure?.Invoke(patch);
            return patch;
        }

        private static SubtractiveVoice Voice(SynthSettings patch, float frequency = 220)
        {
            var voice = new SubtractiveVoice(patch, waveform => Waveforms[waveform]) { Frequency = frequency, Amplitude = 1 };
            voice.NoteOn();
            return voice;
        }

        private static float[] Render(SubtractiveVoice voice, double seconds)
        {
            var samples = new float[(int) (seconds * SampleRate)];

            for (var offset = 0; offset < samples.Length; offset += 256)
            {
                voice.Read(samples.AsSpan(offset, Math.Min(256, samples.Length - offset)));
            }

            return samples;
        }

        private static double Rms(IEnumerable<float> samples) => Math.Sqrt(samples.Average(s => (double) s * s));

        /// <summary>
        /// How much a signal changes from sample to sample, relative to its level: high for bright sounds.
        /// </summary>
        private static double Brightness(float[] samples)
        {
            double change = Rms(samples.Zip(samples.Skip(1), (a, b) => b - a));
            return change / Math.Max(1e-9, Rms(samples));
        }

        /// <summary>
        /// How much the level varies over 20 ms windows (coefficient of variation): 0 for a steady sound.
        /// </summary>
        private static double LevelVariation(float[] samples)
        {
            const int window = SampleRate / 50;
            double[] levels = Enumerable.Range(0, samples.Length / window).Select(i => Rms(samples.Skip(i * window).Take(window))).ToArray();
            double mean = levels.Average();
            return Math.Sqrt(levels.Average(l => (l - mean) * (l - mean))) / mean;
        }

        [Test]
        public void ALowPassFilter_DarkensTheSound()
        {
            double open = Brightness(Render(Voice(Patch()), 0.3));
            double closed = Brightness(Render(Voice(Patch(p => p.Cutoff = 300)), 0.3));

            // A pure 220 Hz tone still scores about 0.031, so "dark" is close to that floor: about a third of open.
            Assert.That(closed, Is.LessThan(open / 2.5));
        }

        [Test]
        public void AHighPassFilter_RemovesLowFrequencies()
        {
            double open = Rms(Render(Voice(Patch(p => p.Waveform = Waveform.Sine)), 0.3).Skip(4410));
            double highPassed = Rms(Render(Voice(Patch(p =>
            {
                p.Waveform = Waveform.Sine;
                p.FilterType = FilterType.HighPass;
                p.Cutoff = 3000;
            })), 0.3).Skip(4410));

            Assert.That(highPassed, Is.LessThan(open * 0.1));
        }

        [Test]
        public void Resonance_EmphasisesTheCutoff()
        {
            double plain = Rms(Render(Voice(Patch(p => p.Cutoff = 1200)), 0.3).Skip(4410));
            double resonant = Rms(Render(Voice(Patch(p =>
            {
                p.Cutoff = 1200;
                p.Resonance = 0.9;
            })), 0.3).Skip(4410));

            Assert.That(resonant, Is.GreaterThan(plain * 1.3));
        }

        [Test]
        public void TheFilterEnvelope_OpensTheFilterThenClosesIt()
        {
            SubtractiveVoice voice = Voice(Patch(p =>
            {
                p.Cutoff = 200;
                p.FilterEnvelopeAmount = 4;
                p.FilterAttackSeconds = 0.001;
                p.FilterDecaySeconds = 0.6;
                p.FilterSustainLevel = 0;
            }));

            // The envelope is exponential (fast at first), so compare the first few milliseconds with long after.
            float[] samples = Render(voice, 1.2);
            double early = Brightness(samples.Skip(88).Take(441).ToArray());
            double late = Brightness(samples.Skip(SampleRate).ToArray());

            // Late is near the pure-tone floor (~0.031), so twice as bright is a large difference.
            Assert.That(early, Is.GreaterThan(late * 1.8));
        }

        [Test]
        public void DetunedOscillators_Beat()
        {
            double steady = LevelVariation(Render(Voice(Patch(p => p.OscillatorMix = 0.5)), 1.0));
            double detuned = LevelVariation(Render(Voice(Patch(p =>
            {
                p.OscillatorMix = 0.5;
                p.Oscillator2Detune = 25;
            })), 1.0));

            Assert.That(steady, Is.LessThan(0.05));
            Assert.That(detuned, Is.GreaterThan(0.15));
        }

        [Test]
        public void AnLfoOnPitch_IsVibrato()
        {
            // Zero crossings of a sine: evenly spaced without vibrato, uneven with it.
            double Spread(SynthSettings patch)
            {
                float[] samples = Render(Voice(patch, 440), 1.0);
                List<int> crossings = Enumerable.Range(1, samples.Length - 1).Where(i => samples[i - 1] < 0 && samples[i] >= 0).ToList();
                double[] periods = crossings.Zip(crossings.Skip(1), (a, b) => (double) (b - a)).ToArray();
                return periods.Max() - periods.Min();
            }

            double plain = Spread(Patch(p => p.Waveform = Waveform.Sine));
            double vibrato = Spread(Patch(p =>
            {
                p.Waveform = Waveform.Sine;
                p.LfoTarget = LfoTarget.Pitch;
                p.LfoDepth = 1;
            }));

            Assert.That(vibrato, Is.GreaterThan(plain + 4), "Periods vary by several samples.");
        }

        [Test]
        public void AnLfoOnVolume_IsTremolo()
        {
            double tremolo = LevelVariation(Render(Voice(Patch(p =>
            {
                p.LfoTarget = LfoTarget.Volume;
                p.LfoDepth = 1;
                p.LfoRate = 6;
            })), 1.0));

            Assert.That(tremolo, Is.GreaterThan(0.2));
        }

        [Test]
        public void MaximumResonance_WithASweepingFilter_StaysStable()
        {
            float[] samples = Render(Voice(Patch(p =>
            {
                p.Cutoff = 800;
                p.Resonance = 1;
                p.LfoTarget = LfoTarget.Filter;
                p.LfoDepth = 1;
                p.LfoRate = 8;
                p.NoiseLevel = 1;
            })), 1.0);

            Assert.That(samples.All(float.IsFinite), Is.True);
            Assert.That(samples.Max(Math.Abs), Is.LessThan(10f));
        }

        [Test]
        public void ChangingThePatch_DoesNotAffectANotePlaying()
        {
            SynthSettings patch = Patch(p => p.Cutoff = 500);
            SubtractiveVoice playing = Voice(patch);
            SubtractiveVoice reference = Voice(Patch(p => p.Cutoff = 500));
            Render(playing, 0.05);
            Render(reference, 0.05);

            patch.Cutoff = 18000;

            Assert.That(Render(playing, 0.05), Is.EqualTo(Render(reference, 0.05)));
        }

        [Test]
        public void NoteOff_ReleasesThenFinishes()
        {
            SubtractiveVoice voice = Voice(Patch(p => p.ReleaseSeconds = 0.05));
            Render(voice, 0.1);

            voice.NoteOff();
            Render(voice, 0.2);

            Assert.That(voice.IsFinished, Is.True);
        }

        [TestCaseSource(nameof(Presets))]
        public void EveryPreset_PlaysCleanly(SynthPreset preset)
        {
            SubtractiveVoice voice = Voice(preset.CreatePatch(), 261.63f);
            float[] held = Render(voice, 0.8);
            voice.NoteOff();
            float[] released = Render(voice, 3.2);

            Assert.That(held.Concat(released).All(float.IsFinite), Is.True);
            Assert.That(held.Max(Math.Abs), Is.GreaterThan(0.05f), "Audible.");
            Assert.That(held.Max(Math.Abs), Is.LessThan(2f), "Leaves the limiter little to do.");
            Assert.That(voice.IsFinished, Is.True, "Fades out after release.");
        }
    }
}
