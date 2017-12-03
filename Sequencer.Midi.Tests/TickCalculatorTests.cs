using NUnit.Framework;
using Sequencer.Domain;

namespace Sequencer.Midi.Tests
{
    [TestFixture]
    public class TickCalculatorTests
    {
        private static readonly object[] TickPositionCases =
        {
            new object[] { 0, new Position(1, 1, 1) },
            new object[] { 6, new Position(1, 1, 2) },
            new object[] { 24, new Position(1, 2, 1) },
            new object[] { 120, new Position(2, 2, 1) }
        };

        [Test]
        [TestCaseSource(nameof(TickPositionCases))]
        public void AssertMidiNumberFromPitch(int tick, Position expectedPosition)
        {
            var tickCalulcator = new TickCalculator();
            IPosition position = tickCalulcator.CalculatePositionFromTick(tick, 24);
            Assert.AreEqual(expectedPosition, position);
        }
    }
}