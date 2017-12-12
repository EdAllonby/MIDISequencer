using Moq;
using NUnit.Framework;
using Sequencer.Domain;
using Sequencer.Domain.Settings;

namespace Sequencer.Midi.Tests
{
    [TestFixture]
    public class TickCalculatorTests
    {
        private static readonly object[] TickPositionCases =
        {
            new object[] { 0, new Position(1, 1, 1) },
            new object[] { 96, new Position(1, 1, 2) },
            new object[] { 192, new Position(1, 1, 3) },
            new object[] { 960, new Position(1, 3, 3) }
        };

        [Test]
        [TestCaseSource(nameof(TickPositionCases))]
        public void AssertMidiNumberFromPitch(int tick, Position expectedPosition)
        {
            var musicalSettingsStub = new Mock<IMusicalSettings>();
            musicalSettingsStub.Setup(x => x.TimeSignature).Returns(TimeSignature.FourFour);
            musicalSettingsStub.Setup(x => x.TicksPerQuarterNote).Returns(96);

            var tickCalulcator = new TickCalculator(musicalSettingsStub.Object);

            IPosition position = tickCalulcator.CalculatePositionFromTick(tick);

            Assert.AreEqual(expectedPosition, position);
        }
    }
}