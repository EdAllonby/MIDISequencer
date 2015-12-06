using NUnit.Framework;

namespace Sequencer.Tests
{
    [TestFixture]
    public class PositionTests
    {
        [Test]
        public void Position_1_1_1_ShouldReturnSumOf1()
        {
            var position = new Position(1, 1, 1);
            var actual = position.SummedBeat(4, 4);
            Assert.AreEqual(1, actual);
        }

        [Test]
        public void Position_1_1_2_ShouldReturnSumOf2()
        {
            var position = new Position(1, 1, 2);
            var actual = position.SummedBeat(4, 4);
            Assert.AreEqual(2, actual);
        }

        [Test]
        public void Position_1_2_1_ShouldReturnSumOf5()
        {
            var position = new Position(1, 2, 1);
            var actual = position.SummedBeat(4, 4);
            Assert.AreEqual(5, actual);
        }

        [Test]
        public void Position_1_2_3_ShouldReturnSumOf7()
        {
            var position = new Position(1, 2, 3);
            var actual = position.SummedBeat(4, 4);
            Assert.AreEqual(7, actual);
        }


        [Test]
        public void Position_1_4_4_ShouldReturnSumOf16()
        {
            var position = new Position(1, 4, 4);
            var actual = position.SummedBeat(4, 4);
            Assert.AreEqual(16, actual);
        }

        [Test]
        public void Position_2_1_1_ShouldReturnSumOf17()
        {
            var position = new Position(2, 1, 1);
            var actual = position.SummedBeat(4, 4);
            Assert.AreEqual(17, actual);
        }

        [Test]
        public void Position_4_4_4_ShouldReturnSumOf64()
        {
            var position = new Position(4, 4, 4);
            var actual = position.SummedBeat(4, 4);
            Assert.AreEqual(64, actual);
        }
    }
}