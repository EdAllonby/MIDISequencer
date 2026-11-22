using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Sequencer.Domain.Drums;

namespace Sequencer.Domain.Tests
{
    [TestFixture]
    public class DrumTests
    {
        private static IEnumerable<DrumLoop> Loops => DrumLoopLibrary.All;

        [TestCaseSource(nameof(Loops))]
        public void EveryLoop_ParsesAndHasHits(DrumLoop loop)
        {
            DrumPattern pattern = loop.ToPattern();

            Assert.That(pattern.HitCount, Is.GreaterThan(0));
            Assert.That(pattern.IsHit(DrumSound.Kick, 0), Is.True, "Every loop starts on a kick.");
        }

        [Test]
        public void OneBarLines_RepeatForFourBars()
        {
            var loop = new DrumLoop("Test", "", 120, new Dictionary<DrumSound, string> { [DrumSound.Kick] = "x... .... .... ...." });

            DrumPattern pattern = loop.ToPattern();

            Assert.That(Enumerable.Range(0, DrumPattern.StepCount).Where(step => pattern.IsHit(DrumSound.Kick, step)), Is.EqualTo(new[] { 0, 16, 32, 48 }));
        }

        [Test]
        public void CapitalX_IsAnAccent()
        {
            var loop = new DrumLoop("Test", "", 120, new Dictionary<DrumSound, string> { [DrumSound.Snare] = "X.x. .... .... ...." });

            DrumPattern pattern = loop.ToPattern();

            Assert.That(pattern.VelocityAt(DrumSound.Snare, 0), Is.EqualTo(DrumPattern.AccentVelocity));
            Assert.That(pattern.VelocityAt(DrumSound.Snare, 2), Is.EqualTo(DrumPattern.NormalVelocity));
        }

        [TestCase("x... ....")]
        [TestCase("x... .... .... ...o")]
        public void BadLines_AreRejected(string line)
        {
            var loop = new DrumLoop("Test", "", 120, new Dictionary<DrumSound, string> { [DrumSound.Kick] = line });

            Assert.Throws<FormatException>(() => loop.ToPattern());
        }

        [Test]
        public void SettingAStep_RaisesChanged_OnlyWhenItChanges()
        {
            var pattern = new DrumPattern();
            var changes = 0;
            pattern.Changed += (sender, args) => changes++;

            pattern.SetVelocity(DrumSound.Clap, 4, 96);
            pattern.SetVelocity(DrumSound.Clap, 4, 96);

            Assert.That(changes, Is.EqualTo(1));
        }

        [Test]
        public void Clone_IsIndependent()
        {
            var pattern = new DrumPattern();
            pattern.SetVelocity(DrumSound.Kick, 0, 96);

            DrumPattern copy = pattern.Clone();
            pattern.Clear();

            Assert.That(copy.IsHit(DrumSound.Kick, 0), Is.True);
            Assert.That(pattern.IsEmpty, Is.True);
        }
    }
}
