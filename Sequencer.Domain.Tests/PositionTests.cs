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
        
        private static readonly object[] PreviousPositionCases =
        {
            new object[] { new Position(1, 1, 2), new Position(1, 1, 1) },
            new object[] { new Position(3, 1, 2), new Position(3, 1, 1) },
            new object[] { new Position(5, 1, 1), new Position(4, 4, 4) },
            new object[] { new Position(2, 4, 1), new Position(2, 3, 4) }
        };

        private static readonly object[] CompareToCases =
        {
            new object[] { new Position(1, 1, 1), new Position(1, 1, 2), -1 },
            new object[] { new Position(3, 1, 1), new Position(3, 1, 2), -1 },
            new object[] { new Position(3, 1, 2), new Position(3, 1, 1), 1 },
            new object[] { new Position(3, 1, 2), new Position(3, 1, 2), 0 },
            new object[] { new Position(3, 1, 2, 1234), new Position(3, 1, 2, 1234), 0 },
            new object[] { new Position(3, 1, 2, 1234), new Position(3, 1, 2, 1235), -1 },
            new object[] { new Position(3, 1, 2, 1235), new Position(3, 1, 2, 1234), 1 },
            new object[] { new Position(3, 1, 3), new Position(3, 1, 2, 12340000), 1 },
            new object[] { new Position(3, 2, 3), new Position(3, 1, 3), 1 }
        };

        private static readonly object[] IsGreaterThanCases =
        {
            new object[] { new Position(1, 1, 1), new Position(1, 1, 2), false },
            new object[] { new Position(3, 1, 1), new Position(3, 1, 2), false },
            new object[] { new Position(3, 1, 2), new Position(3, 1, 1), true },
            new object[] { new Position(3, 1, 2, 1), new Position(3, 1, 2, 1), false },
            new object[] { new Position(3, 1, 2, 123), new Position(3, 1, 2, 122), true },
            new object[] { new Position(3, 1, 2, 122), new Position(3, 1, 2, 123), false }
        };

        private static readonly object[] IsGreaterThanOrEqualCases =
        {
            new object[] { new Position(1, 1, 1), new Position(1, 1, 2), false },
            new object[] { new Position(3, 1, 1), new Position(3, 1, 2), false },
            new object[] { new Position(3, 1, 2), new Position(3, 1, 1), true },
            new object[] { new Position(3, 1, 2, 1), new Position(3, 1, 2, 1), true },
            new object[] { new Position(3, 1, 2, 123), new Position(3, 1, 2, 122), true },
            new object[] { new Position(3, 1, 2, 122), new Position(3, 1, 2, 123), false }
        };

        private static readonly object[] IsLessThanCases =
        {
            new object[] { new Position(1, 1, 1), new Position(1, 1, 2), true },
            new object[] { new Position(3, 1, 1), new Position(3, 1, 2), true },
            new object[] { new Position(3, 1, 2), new Position(3, 1, 1), false },
            new object[] { new Position(3, 1, 2, 1), new Position(3, 1, 2, 1), false },
            new object[] { new Position(3, 1, 2, 123), new Position(3, 1, 2, 122), false },
            new object[] { new Position(3, 1, 2, 122), new Position(3, 1, 2, 123), true }
        };

        private static readonly object[] IsLessThanOrEqualCases =
        {
            new object[] { new Position(1, 1, 1), new Position(1, 1, 2), true },
            new object[] { new Position(3, 1, 1), new Position(3, 1, 2), true },
            new object[] { new Position(3, 1, 2), new Position(3, 1, 1), false },
            new object[] { new Position(3, 1, 2, 1), new Position(3, 1, 2, 1), true },
            new object[] { new Position(3, 1, 2, 123), new Position(3, 1, 2, 122), false },
            new object[] { new Position(3, 1, 2, 122), new Position(3, 1, 2, 123), true }
        };

        [Test]
        [TestCaseSource(nameof(BeatPositionCases))]
        public void BeatShouldReturnCorrectPosition(int beat, IPosition expectedPosition)
        {
            const int ticksPerQuarterNote = 96;
            int tick = (beat - 1) * ticksPerQuarterNote;

            IPosition actualPosition = Position.PositionFromTick(tick, standardTimeSignature, ticksPerQuarterNote);
            Assert.AreEqual(expectedPosition, actualPosition);
        }

        [Test]
        [TestCaseSource(nameof(CompareToCases))]
        public void CompareToPositionTests(IPosition first, IPosition second, int expected)
        {
            int actual = first.CompareTo(second);

            Assert.AreEqual(expected, actual);
        }

        [Test]
        [TestCaseSource(nameof(IsGreaterThanOrEqualCases))]
        public void IsGreaterThanOrEqualPositionTests(IPosition first, IPosition second, bool expected)
        {
            bool actual = first.IsGreaterThanOrEqual(second);

            Assert.AreEqual(expected, actual);
        }

        [Test]
        [TestCaseSource(nameof(IsGreaterThanCases))]
        public void IsGreaterThanPositionTests(IPosition first, IPosition second, bool expected)
        {
            bool actual = first.IsGreaterThan(second);

            Assert.AreEqual(expected, actual);
        }

        [Test]
        [TestCaseSource(nameof(IsLessThanOrEqualCases))]
        public void IsLessThanOrEqualPositionTests(IPosition first, IPosition second, bool expected)
        {
            bool actual = first.IsLessThanOrEqual(second);

            Assert.AreEqual(expected, actual);
        }

        [Test]
        [TestCaseSource(nameof(IsLessThanCases))]
        public void IsLessThanPositionTests(IPosition first, IPosition second, bool expected)
        {
            bool actual = first.IsLessThan(second);

            Assert.AreEqual(expected, actual);
        }

        [Test]
        [TestCaseSource(nameof(NextPositionCases))]
        public void NextPositionShouldBeCorrect(IPosition initialPosition, IPosition expectedNextPosition)
        {
            IPosition actualNextPosition = initialPosition.NextPosition(NoteResolution.Quarter, standardTimeSignature, 96);

            Assert.AreEqual(expectedNextPosition, actualNextPosition);
        }

        [Test]
        public void NullSecondPosition_ReturnsNotEqual()
        {
            var firstPosition = new Position(9, 1, 2, 21);

            Assert.IsFalse(firstPosition.Equals(null));
            Assert.AreNotEqual(firstPosition.GetHashCode(), null);
        }

        [Test]
        public void NullSecondPositionObject_ReturnsNotEqual()
        {
            var firstPosition = new Position(9, 1, 2, 21);

            Assert.IsFalse(firstPosition.Equals((object) null));
            Assert.AreNotEqual(firstPosition.GetHashCode(), null);
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
        public void Position_2_1_4_93_IsSmallerThan_Position_2_1_4_94()
        {
            var firstPosition = new Position(2, 1, 4, 93);
            var secondPosition = new Position(2, 1, 4, 94);

            Assert.IsTrue(firstPosition < secondPosition);
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
        public void Position_9_1_2_21_IsEqualTo_Position_9_1_2_21()
        {
            var firstPosition = new Position(9, 1, 2, 21);
            var secondPosition = new Position(9, 1, 2, 21);

            Assert.IsTrue(firstPosition.Equals(secondPosition));
            Assert.AreEqual(firstPosition.GetHashCode(), secondPosition.GetHashCode());
        }

        [Test]
        [TestCaseSource(nameof(PositionSumCases))]
        public void PositionShouldReturnCorrectSummedBeat(IPosition position, int expectedSum)
        {
            int actual = position.SummedBeat(standardTimeSignature);
            Assert.AreEqual(expectedSum, actual);
        }

        [Test]
        [TestCaseSource(nameof(PreviousPositionCases))]
        public void PreviousPositionShouldBeCorrect(IPosition initialPosition, IPosition expectedPreviousPosition)
        {
            IPosition actualPreviousPosition = initialPosition.PreviousPosition(NoteResolution.Quarter, standardTimeSignature, 96);

            Assert.AreEqual(expectedPreviousPosition, actualPreviousPosition);
        }

        [Test]
        public void ReferenceEquals_ReturnsTrue()
        {
            var firstPosition = new Position(9, 1, 2, 21);

            Assert.IsTrue(firstPosition.Equals(firstPosition));
            Assert.AreEqual(firstPosition.GetHashCode(), firstPosition.GetHashCode());
        }


        [Test]
        public void ReferenceEqualsObject_ReturnsTrue()
        {
            var firstPosition = new Position(9, 1, 2, 21);

            Assert.IsTrue(firstPosition.Equals((object) firstPosition));
        }
    }
}