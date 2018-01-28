using Moq;
using NUnit.Framework;

namespace Sequencer.Domain.Tests
{
    [TestFixture]
    public class ToneTests
    {
        [Test]
        public void EndPosition_CannotBeSet_LessThanStartPosition()
        {
            var startPosition = new Position(1, 1, 1);
            var endPosition = new Position(0, 1, 2);
            var tone = new Tone(It.IsAny<Pitch>(), It.IsAny<Velocity>(), startPosition, endPosition);


            Assert.AreEqual(startPosition, tone.EndPosition);
        }

        [Test]
        public void EndPosition_IsSet_WhenGreaterThanStartPosition()
        {
            var startPosition = new Position(1, 1, 1);
            var endPosition = new Position(1, 1, 2);
            var tone = new Tone(It.IsAny<Pitch>(), It.IsAny<Velocity>(), startPosition, endPosition);

            Assert.AreEqual(endPosition, tone.EndPosition);
        }

        [Test]
        public void Pitch_IsSet()
        {
            var pitch = new Pitch(Note.A, 3);
            var tone = new Tone(pitch, It.IsAny<Velocity>(), new Position(0, 0, 0), new Position(0, 0, 0));

            Assert.AreEqual(pitch, tone.Pitch);
        }

        [Test]
        public void StartPosition_IsSet()
        {
            var startPosition = new Position(4, 1, 2, 120);

            var tone = new Tone(It.IsAny<Pitch>(), It.IsAny<Velocity>(), startPosition, new Position(0, 0, 0));

            Assert.AreEqual(startPosition, tone.StartPosition);
        }

        [Test]
        public void Velocity_IsSet()
        {
            var velocity = new Velocity(112);
            var tone = new Tone(It.IsAny<Pitch>(), velocity, new Position(0, 0, 0), new Position(0, 0, 0));

            Assert.AreEqual(velocity, tone.Velocity);
        }
    }
}