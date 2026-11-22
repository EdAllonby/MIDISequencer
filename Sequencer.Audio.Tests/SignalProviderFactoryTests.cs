using System;
using System.Linq;
using Moq;
using NUnit.Framework;
using Sequencer.Domain;
using Sequencer.Domain.Settings;

namespace Sequencer.Audio.Tests
{
    [TestFixture]
    public class SignalProviderFactoryTests
    {
        private static ISequencedNote ConcertA()
        {
            var tone = new Tone(new Pitch(Note.A, 4), new Velocity(127), new Position(1, 1, 1), new Position(1, 1, 2));
            return Mock.Of<ISequencedNote>(n => n.Tone == tone);
        }

        /// <summary>
        /// RMS divided by peak, which identifies the waveform's shape: about 0.9 for this band-limited square
        /// (its edges overshoot slightly), 0.71 for a sine, and 0.58 for triangle and sawtooth.
        /// </summary>
        private static double RmsToPeak(Waveform waveform)
        {
            var settings = new SynthSettings { Waveform = waveform, AttackSeconds = 0.001, DecaySeconds = 0.001, SustainLevel = 1 };
            var frequency = Mock.Of<IFrequencyCalculator>(f => f.PitchFrequency(It.IsAny<Pitch>()) == 441.0);
            ISignalProvider voice = new SignalProviderFactory(frequency).CreateSignalProvider(ConcertA(), settings);
            voice.NoteOn();

            var samples = new float[settings.SampleRate / 10];
            voice.Read(samples);
            float[] settled = samples.Skip(1000).ToArray();
            float peak = settled.Max(Math.Abs);

            double rms = Math.Sqrt(settled.Average(s => (double) s * s));
            return rms / peak;
        }

        [Test]
        public void Square_IsMostlyAtFullLevel()
        {
            Assert.That(RmsToPeak(Waveform.Square), Is.GreaterThan(0.8));
        }

        [Test]
        public void Sine_HasTheShapeOfASine()
        {
            Assert.That(RmsToPeak(Waveform.Sine), Is.EqualTo(Math.Sqrt(0.5)).Within(0.01));
        }

        [TestCase(Waveform.Sawtooth)]
        [TestCase(Waveform.Triangle)]
        public void RampWaveforms_HaveTheRmsOfARamp(Waveform waveform)
        {
            Assert.That(RmsToPeak(waveform), Is.InRange(0.5, 0.65));
        }
    }
}
