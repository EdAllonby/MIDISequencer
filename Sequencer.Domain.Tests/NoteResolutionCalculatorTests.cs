using NUnit.Framework;

namespace Sequencer.Domain.Tests
{
    [TestFixture]
    public class NoteResolutionCalculatorTests
    {
        private static readonly object[] NoteResolutionCases =
        {
            new object[] { NoteResolution.Quarter, 96, 96 },
            new object[] { NoteResolution.Whole, 96, 384 },
            new object[] { NoteResolution.Half, 96, 192 },
            new object[] { NoteResolution.Eighth, 96, 48 },
            new object[] { NoteResolution.Sixteenth, 96, 24 },
        };
        
        [Test]
        [TestCaseSource(nameof(NoteResolutionCases))]
        public void GetTicksForResolutionTests(NoteResolution noteResolution, int tick, int expectedTicks)
        {
            int actualTicks = NoteResolutionCalculator.GetTicksForResolution(noteResolution, tick);

            Assert.AreEqual(expectedTicks, actualTicks);
        }
    }
}
