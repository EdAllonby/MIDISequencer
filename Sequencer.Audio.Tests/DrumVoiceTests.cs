using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Sequencer.Domain.Drums;

namespace Sequencer.Audio.Tests
{
    [TestFixture]
    public class DrumVoiceTests
    {
        private const int SampleRate = 44100;

        private static IEnumerable<DrumSound> Sounds => DrumPattern.AllSounds;

        private static float[] Render(DrumVoice voice, double seconds)
        {
            var samples = new float[(int) (seconds * SampleRate)];

            for (var offset = 0; offset < samples.Length; offset += 256)
            {
                voice.Read(samples.AsSpan(offset, Math.Min(256, samples.Length - offset)));
            }

            return samples;
        }

        [TestCaseSource(nameof(Sounds))]
        public void EverySound_IsAudibleAndWithinFullScale(DrumSound sound)
        {
            float[] samples = Render(new DrumVoice(sound, 127, SampleRate), 0.2);

            float peak = samples.Max(Math.Abs);
            Assert.That(peak, Is.GreaterThan(0.1f), "Audible.");
            Assert.That(peak, Is.LessThanOrEqualTo(1.2f), "Leaves the limiter little to do.");
        }

        [Test]
        public void Kit_IsBalanced_HatsSitUnderTheKickAndSnare()
        {
            float Peak(DrumSound sound) => Render(new DrumVoice(sound, 127, SampleRate), 0.2).Max(Math.Abs);

            Assert.That(Peak(DrumSound.ClosedHat), Is.InRange(0.25f, 0.6f));
            Assert.That(Peak(DrumSound.OpenHat), Is.InRange(0.25f, 0.6f));
            Assert.That(Peak(DrumSound.ClosedHat), Is.LessThan(Peak(DrumSound.Kick)));
            Assert.That(Peak(DrumSound.ClosedHat), Is.LessThan(Peak(DrumSound.Snare)));
        }

        [TestCaseSource(nameof(Sounds))]
        public void EverySound_FinishesAfterItsLength(DrumSound sound)
        {
            var voice = new DrumVoice(sound, 127, SampleRate);

            Render(voice, DrumVoice.LengthOf(sound) + 0.01);

            Assert.That(voice.IsFinished, Is.True);
            Assert.That(voice.Read(new float[64]), Is.EqualTo(0), "A finished hit tells the mixer to drop it.");
        }

        [TestCaseSource(nameof(Sounds))]
        public void EverySound_IsTheSameEveryTime(DrumSound sound)
        {
            Assert.That(Render(new DrumVoice(sound, 100, SampleRate), 0.05), Is.EqualTo(Render(new DrumVoice(sound, 100, SampleRate), 0.05)));
        }

        [Test]
        public void Velocity_ScalesTheLevel()
        {
            float loud = Render(new DrumVoice(DrumSound.Kick, 127, SampleRate), 0.1).Max(Math.Abs);
            float quiet = Render(new DrumVoice(DrumSound.Kick, 64, SampleRate), 0.1).Max(Math.Abs);

            Assert.That(quiet / loud, Is.EqualTo(64 / 127f).Within(0.01));
        }

        [Test]
        public void Choke_SilencesAnOpenHatWithinMilliseconds()
        {
            var voice = new DrumVoice(DrumSound.OpenHat, 127, SampleRate);
            Render(voice, 0.05);

            voice.Choke();
            Render(voice, 0.01);

            Assert.That(voice.IsFinished, Is.True);
        }
    }
}
