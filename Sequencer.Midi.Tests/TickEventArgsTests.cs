using NUnit.Framework;
using NUnit.Framework.Legacy;

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

            ClassicAssert.AreEqual(expected, args.CurrentTick);
        }
    }
}