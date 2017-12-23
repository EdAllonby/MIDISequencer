using Moq;
using NUnit.Framework;
using Sequencer.Domain.Settings;

namespace Sequencer.Domain.Tests
{
    [TestFixture]
    public class PitchAndPositionCalculatorTests
    {
        private static readonly object[] PositionCases =
        {
            new object[] { new Position(1, 1, 1), new Position(1, 1, 1), 96, 0 },
            new object[] { new Position(1, 1, 1), new Position(1, 1, 1, 1), 96, 1 },
            new object[] { new Position(1, 1, 1), new Position(1, 1, 2), 96, 96 },
            new object[] { new Position(1, 1, 1), new Position(1, 1, 2), 24, 24 },
            new object[] { new Position(1, 1, 1), new Position(1, 1, 2), 24, 24 },
            new object[] { new Position(1, 1, 1), new Position(1, 1, 2, 1), 96, 97 },
            new object[] { new Position(2, 1, 1), new Position(3, 1, 1), 96, 1536 },
            new object[] { new Position(2, 1, 1), new Position(4, 1, 2), 96, 3168 }
        };

        private static readonly object[] PitchCases =
        {
            new object[] { new Pitch(Note.A, 2), new Pitch(Note.A, 2), 0 },
            new object[] { new Pitch(Note.A, 2), new Pitch(Note.ASharp, 2), 1 },
            new object[] { new Pitch(Note.A, 2), new Pitch(Note.B, 2), 2 },
            new object[] { new Pitch(Note.A, 2), new Pitch(Note.C, 3), 3 },
            new object[] { new Pitch(Note.E, 3), new Pitch(Note.F, 3), 1 },
            new object[] { new Pitch(Note.A, 2), new Pitch(Note.A, 3), 12 }
        };

        [Test]
        [TestCaseSource(nameof(PitchCases))]
        public void FindStepsFromPitchesTest(Pitch firstPitch, Pitch secondPitch, int expectedSteps)
        {
            var mockMusicalSettings = new Mock<IMusicalSettings>();

            var calculator = new PitchAndPositionCalculator(mockMusicalSettings.Object);

            double actualSteps = calculator.FindStepsFromPitches(firstPitch, secondPitch);

            Assert.AreEqual(expectedSteps, actualSteps);
        }

        [Test]
        [TestCaseSource(nameof(PositionCases))]
        public void TicksBetweenPositionsTest(Position firstPosition, Position secondPosition, int ticksPerQuarterNote, int expectedTicks)
        {
            var mockMusicalSettings = new Mock<IMusicalSettings>();

            mockMusicalSettings.Setup(x => x.TimeSignature).Returns(TimeSignature.FourFour);
            mockMusicalSettings.Setup(x => x.TicksPerQuarterNote).Returns(ticksPerQuarterNote);

            var calculator = new PitchAndPositionCalculator(mockMusicalSettings.Object);

            double actualTicks = calculator.FindTicksBetweenPositions(firstPosition, secondPosition);

            Assert.AreEqual(expectedTicks, actualTicks);
        }
    }
}