using System;
using Moq;
using NUnit.Framework;
using Sequencer.Audio.Calculator;
using Sequencer.Domain.Settings;

namespace Sequencer.Audio.Tests
{
    [TestFixture]
    public class SignalProviderTests
    {
        private const int SampleRate = 1000;

        private static IAudioSettings Settings(double attack = 0.001, double decay = 0.001, double sustain = 1.0, double release = 0.05)
        {
            var settings = new Mock<IAudioSettings>();
            settings.Setup(x => x.SampleRate).Returns(SampleRate);
            settings.Setup(x => x.AttackSeconds).Returns(attack);
            settings.Setup(x => x.DecaySeconds).Returns(decay);
            settings.Setup(x => x.SustainLevel).Returns(sustain);
            settings.Setup(x => x.ReleaseSeconds).Returns(release);
            return settings.Object;
        }

        [Test]
        public void Read_HasNoPhaseJumps_AcrossBuffersAndSeconds()
        {
            // The old implementation reset its sample counter every second, which jumped the phase for any
            // frequency that is not a whole number of hertz (issue #15).
            const float frequency = 3.7f;
            var voice = new SignalProvider(new SineWaveCalculator(), Settings()) { Frequency = frequency, Amplitude = 1 };
            voice.NoteOn();

            var samples = new float[3 * SampleRate];

            for (var offset = 0; offset < samples.Length; offset += 128)
            {
                voice.Read(samples.AsSpan(offset, Math.Min(128, samples.Length - offset)));
            }

            double largestPossibleStep = 2 * Math.PI * frequency / SampleRate;

            // Skip the attack and decay, which take a few samples to settle at full volume.
            for (var n = 20; n < samples.Length; n++)
            {
                Assert.That(Math.Abs(samples[n] - samples[n - 1]), Is.LessThanOrEqualTo(largestPossibleStep * 1.01),
                    $"Discontinuity at sample {n}");
            }
        }

        [Test]
        public void NoteOff_FadesOutThenFinishes()
        {
            // Release lasts 0.05 s = 50 samples.
            var voice = new SignalProvider(new SineWaveCalculator(), Settings()) { Frequency = 10, Amplitude = 1 };
            voice.NoteOn();
            var buffer = new float[100];
            voice.Read(buffer);

            voice.NoteOff();

            Assert.That(voice.IsFinished, Is.False, "Releasing is not finished.");
            Assert.That(voice.Read(buffer), Is.EqualTo(buffer.Length), "The release stage is still played.");

            for (var i = 0; i < 20 && !voice.IsFinished; i++)
            {
                voice.Read(buffer);
            }

            Assert.That(voice.IsFinished, Is.True);
            Assert.That(voice.Read(buffer), Is.EqualTo(0), "A finished voice tells the mixer to remove it.");
        }

        [Test]
        public void WithoutNoteOff_KeepsPlaying()
        {
            var voice = new SignalProvider(new SineWaveCalculator(), Settings()) { Frequency = 10, Amplitude = 1 };
            voice.NoteOn();
            var buffer = new float[SampleRate];

            for (var i = 0; i < 5; i++)
            {
                Assert.That(voice.Read(buffer), Is.EqualTo(buffer.Length));
            }

            Assert.That(voice.IsFinished, Is.False);
        }

        [Test]
        public void Amplitude_IgnoresValuesOutOfRange()
        {
            var voice = new SignalProvider(new SineWaveCalculator(), Settings()) { Amplitude = 0.5f };

            voice.Amplitude = 2;

            Assert.That(voice.Amplitude, Is.EqualTo(0.5f));
        }
    }
}
