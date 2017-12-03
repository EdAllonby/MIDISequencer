using NUnit.Framework;

namespace Sequencer.Domain.Tests
{
    [TestFixture]
    public class PositionTests
    {
        private readonly TimeSignature standardTimeSignature = new TimeSignature(4, 4);

        private static readonly object[] PositionSumCases =
        {
            new object[] { new Position(1, 1, 1), 1 },
            new object[] { new Position(1, 1, 2), 2 },
            new object[] { new Position(1, 2, 1), 5 },
            new object[] { new Position(1, 2, 3), 7 },
            new object[] { new Position(1, 4, 4), 16 },
            new object[] { new Position(2, 1, 1), 17 },
            new object[] { new Position(4, 4, 4), 64 }
        };

        private static readonly object[] BeatPositionCases =
        {
            new object[] { 17, new Position(2, 1, 1) },
            new object[] { 16, new Position(1, 4, 4) },
            new object[] { 49, new Position(4, 1, 1) },
            new object[] { 21, new Position(2, 2, 1) },
            new object[] { 22, new Position(2, 2, 2) },
            new object[] { 1, new Position(1, 1, 1) },
            new object[] { 4, new Position(1, 1, 4) },
            new object[] { 5, new Position(1, 2, 1) },
            new object[] { 25, new Position(2, 3, 1) }
        };

        private static readonly object[] NextPositionCases =
        {
            new object[] { new Position(1, 1, 1), new Position(1, 1, 2) },
            new object[] { new Position(3, 1, 1), new Position(3, 1, 2) },
            new object[] { new Position(4, 4, 4), new Position(5, 1, 1) },
            new object[] { new Position(2, 3, 4), new Position(2, 4, 1) }
        };

        [Test]
        [TestCaseSource(nameof(BeatPositionCases))]
        public void BeatShouldReturnCorrectPosition(int beat, IPosition expectedPosition)
        {
            IPosition actualPosition = Position.PositionFromBeat(beat, standardTimeSignature);
            Assert.AreEqual(expectedPosition, actualPosition);
        }

        [Test]
        [TestCaseSource(nameof(NextPositionCases))]
        public void NextPositionShouldBeCorrect(IPosition initialPosition, IPosition expectedNextPosition)
        {
            IPosition actualNextPosition = initialPosition.NextPosition(standardTimeSignature);

            Assert.AreEqual(expectedNextPosition, actualNextPosition);
        }

        [Test]
        public void Position_1_1_1_IsSmallerThan_Position_1_1_2()
        {
            var smallerPosition = new Position(1, 1, 1);
            var largerPosition = new Position(1, 1, 2);

            Assert.IsTrue(smallerPosition < largerPosition);
        }

        [Test]
        public void Position_1_2_1_IsNotLargerThanOrEqualTo_Position_1_3_4()
        {
            var largerPosition = new Position(1, 3, 4);
            var smallerPosition = new Position(1, 2, 1);

            Assert.IsFalse(smallerPosition >= largerPosition);
        }

        [Test]
        public void Position_1_2_4_IsNotLargerThanOrEqualTo_Position_2_4_1()
        {
            var largerPosition = new Position(2, 4, 1);
            var smallerPosition = new Position(1, 2, 4);

            Assert.IsFalse(smallerPosition >= largerPosition);
        }

        [Test]
        public void Position_1_4_2_IsSmallerThanOrEqualTo_Position_2_3_4()
        {
            var smallerPosition = new Position(1, 2, 4);
            var largerPosition = new Position(2, 3, 4);

            Assert.IsTrue(smallerPosition <= largerPosition);
        }

        [Test]
        public void Position_3_3_1_IsEqualTo_Position_3_3_1()
        {
            var firstPosition = new Position(3, 3, 1);
            var secondPosition = new Position(3, 3, 1);

            Assert.IsTrue(firstPosition.Equals(secondPosition));
        }

        [Test]
        public void Position_5_2_1_IsLargerThan_Position_2_2_2()
        {
            var largerPosition = new Position(5, 2, 1);
            var smallerPosition = new Position(2, 2, 2);

            Assert.IsTrue(largerPosition > smallerPosition);
        }

        [Test]
        public void Position_5_2_2_IsLargerThanOrEqualTo_Position_5_2_2()
        {
            var firstPosition = new Position(5, 2, 2);
            var secondPosition = new Position(5, 2, 2);

            Assert.IsTrue(firstPosition >= secondPosition);
        }

        [Test]
        [TestCaseSource(nameof(PositionSumCases))]
        public void PositionShouldReturnCorrectSummedBeat(IPosition position, int expectedSum)
        {
            int actual = position.SummedBeat(standardTimeSignature);
            Assert.AreEqual(expectedSum, actual);
        }
    }
}