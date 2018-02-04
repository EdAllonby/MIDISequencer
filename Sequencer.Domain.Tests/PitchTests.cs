using NUnit.Framework;

namespace Sequencer.Domain.Tests
{
    [TestFixture]
    public class PitchTests
    {
        [Test]
        public void DifferencePitches_AreNotEqual()
        {
            var pitch = new Pitch(Note.A, 2);
            var pitch2 = new Pitch(Note.B, 2);

            Assert.AreNotEqual(pitch, pitch2);
            Assert.AreNotEqual(pitch.GetHashCode(), pitch2.GetHashCode());
        }

        [Test]
        public void DifferencePitchesWithDifferentOctaves_AreNotEqual()
        {
            var pitch = new Pitch(Note.A, 2);
            var pitch2 = new Pitch(Note.A, 3);

            Assert.AreNotEqual(pitch, pitch2);
            Assert.AreNotEqual(pitch.GetHashCode(), pitch2.GetHashCode());
        }

        [Test]
        public void EqualPitches_AreEqual()
        {
            var pitch = new Pitch(Note.A, 2);
            var pitch2 = new Pitch(Note.A, 2);

            Assert.AreEqual(pitch, pitch2);
            Assert.AreEqual(pitch.GetHashCode(), pitch2.GetHashCode());
        }

        [Test]
        public void EqualPitchObjects_AreEqual()
        {
            var pitch = new Pitch(Note.A, 2);
            var pitch2 = new Pitch(Note.A, 2);

            Assert.IsTrue(pitch.Equals((object) pitch2));
        }

        [Test]
        public void ReferenceEqualNullPitches_AreNotEqual()
        {
            var pitch = new Pitch(Note.A, 2);

            Assert.IsFalse(pitch.Equals(null));
        }

        [Test]
        public void ReferenceEqualNullPitchObjects_AreNotEqual()
        {
            var pitch = new Pitch(Note.A, 2);

            Assert.IsFalse(pitch.Equals((object) null));
        }

        [Test]
        public void ReferenceEqualPitches_AreEqual()
        {
            var pitch = new Pitch(Note.A, 2);

            Assert.IsTrue(pitch.Equals(pitch));
        }

        [Test]
        public void ReferenceEqualPitchObjects_AreEqual()
        {
            var pitch = new Pitch(Note.A, 2);

            Assert.IsTrue(pitch.Equals((object) pitch));
        }
    }
}