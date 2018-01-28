using NUnit.Framework;

namespace Sequencer.Midi.Tests
{
    [TestFixture]
    public class TickEventArgsTests
    {
        [Test]
        public void TickArgs_HoldsCurrentTicks()
        {
            var expected = 987;
            var args = new TickEventArgs(expected);

            Assert.AreEqual(expected, args.CurrentTick);
        }
    }
}