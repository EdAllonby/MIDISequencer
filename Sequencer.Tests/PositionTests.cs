using NUnit.Framework;

namespace Sequencer.Tests
{
    [TestFixture]
    public class PositionTests
    {
        private readonly TimeSignature standardTimeSignature = new TimeSignature(4, 4);

        [Test]
        public void Position_1_1_1_ShouldReturnSumOf1()
        {
            var position = new Position(1, 1, 1);
            int actual = position.SummedBeat(standardTimeSignature);
            Assert.AreEqual(1, actual);
        }

        [Test]
        public void Position_1_1_2_ShouldReturnSumOf2()
        {
            var position = new Position(1, 1, 2);
            int actual = position.SummedBeat(standardTimeSignature);
            Assert.AreEqual(2, actual);
        }

        [Test]
        public void Position_1_2_1_ShouldReturnSumOf5()
        {
            var position = new Position(1, 2, 1);
            int actual = position.SummedBeat(standardTimeSignature);
            Assert.AreEqual(5, actual);
        }

        [Test]
        public void Position_1_2_3_ShouldReturnSumOf7()
        {
            var position = new Position(1, 2, 3);
            int actual = position.SummedBeat(standardTimeSignature);
            Assert.AreEqual(7, actual);
        }
        
        [Test]
        public void Position_1_4_4_ShouldReturnSumOf16()
        {
            var position = new Position(1, 4, 4);
            int actual = position.SummedBeat(standardTimeSignature);
            Assert.AreEqual(16, actual);
        }

        [Test]
        public void Position_2_1_1_ShouldReturnSumOf17()
        {
            var position = new Position(2, 1, 1);
            int actual = position.SummedBeat(standardTimeSignature);
            Assert.AreEqual(17, actual);
        }

        [Test]
        public void Position_4_4_4_ShouldReturnSumOf64()
        {
            var position = new Position(4, 4, 4);
            var actual = position.SummedBeat(standardTimeSignature);
            Assert.AreEqual(64, actual);
        }

        [Test]
        public void Beats_17_ShouldReturnPositionMeasure_2()
        {
            Position position = Position.PositionFromBeat(17, standardTimeSignature);
            AssertPosition(position, 2, 1, 1);
        }

        [Test]
        public void Beats_16_ShouldReturnPositionMeasure_1_Bars_4_Beats_4()
        {
            Position position = Position.PositionFromBeat(16, standardTimeSignature);
            AssertPosition(position, 1, 4, 4);
        }

        [Test]
        public void Beats_49_ShouldReturnPositionMeasure_4()
        {
            Position position = Position.PositionFromBeat(49, standardTimeSignature);
            AssertPosition(position, 4, 1, 1);
        }

        [Test]
        public void Beats_21_ShouldReturnPositionMeasure_2_Bars_2()
        {
            Position position = Position.PositionFromBeat(21, standardTimeSignature);
            AssertPosition(position,2,2,1);
        }

        [Test]
        public void Beats_22_ShouldReturnPositionMeasure_2_Bars_2_Beats_2()
        {
            Position position = Position.PositionFromBeat(22, standardTimeSignature);
            AssertPosition(position, 2, 2, 2);
        }
        
        [Test]
        public void Beats_1_ShouldReturnPositionMeasure_1_Bars_1_Beats_1()
        {
            Position position = Position.PositionFromBeat(1, standardTimeSignature);
            AssertPosition(position, 1,1,1);
        }

        [Test]
        public void Beats_4_ShouldReturnPositionMeasure_1_Bars_1_Beats_4()
        {
            Position position = Position.PositionFromBeat(4, standardTimeSignature);
            AssertPosition(position, 1, 1, 4);
        }
        
        [Test]
        public void Beats_5_ShouldReturnPositionMeasure_1_Bars_2_Beats_1()
        {
            Position position = Position.PositionFromBeat(5, standardTimeSignature);
            AssertPosition(position, 1, 2, 1);
        }

        [Test]
        public void Beats_25_ShouldReturnPositionMeasure_2_Bars_2_Beats_1()
        {
            Position position = Position.PositionFromBeat(25, standardTimeSignature);
            AssertPosition(position, 2, 3, 1);
        }

        private static void AssertPosition(Position position, int measure, int bar, int beat)
        {
            Assert.AreEqual(measure, position.Measure);
            Assert.AreEqual(bar, position.Bar);
            Assert.AreEqual(beat, position.Beat);
        }
    }
}