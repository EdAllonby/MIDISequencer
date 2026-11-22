using NUnit.Framework;
using Sequencer.Domain;

namespace Sequencer.ViewModel.Tests
{
    [TestFixture]
    public class MusicalFormatTests
    {
        [TestCase(0, "1.1.1")]
        [TestCase(24, "1.1.2")]
        [TestCase(96, "1.2.1")]
        [TestCase(384, "2.1.1")]
        [TestCase(384 * 3 + 96 * 2 + 24 * 3, "4.3.4")]
        public void Position_IsBarBeatSixteenth(int tick, string expected)
        {
            IPosition position = Domain.Position.PositionFromTick(tick, TimeSignature.FourFour, 96);

            Assert.That(MusicalFormat.Position(position, TimeSignature.FourFour, 96), Is.EqualTo(expected));
        }

        [TestCase(1, "1/16")]
        [TestCase(2, "1/8")]
        [TestCase(3, "3/16")]
        [TestCase(4, "1/4")]
        [TestCase(12, "3/4")]
        [TestCase(16, "1 bar")]
        [TestCase(32, "2 bars")]
        [TestCase(20, "1 bar + 1/4")]
        public void Length_IsAMusicalFraction(int sixteenths, string expected)
        {
            Assert.That(MusicalFormat.Length(sixteenths), Is.EqualTo(expected));
        }
    }
}
