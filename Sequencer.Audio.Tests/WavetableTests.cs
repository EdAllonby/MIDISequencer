using System.Collections.Generic;
using NUnit.Framework;
using NUnit.Framework.Legacy;

namespace Sequencer.Audio.Tests
{
    [TestFixture]
    public class WavetableTests
    {
        [Test]
        public void SampleAtPosition_GetsCorrectSample()
        {
            var samples = new List<float> { 1, 2, 3, 4 };

            var wavetable = new Wavetable(samples);

            ClassicAssert.AreEqual(1, wavetable.SampleAtPosition(0));
            ClassicAssert.AreEqual(2, wavetable.SampleAtPosition(1));
            ClassicAssert.AreEqual(3, wavetable.SampleAtPosition(2));
            ClassicAssert.AreEqual(4, wavetable.SampleAtPosition(3));
        }

        [Test]
        public void SampleAtPosition_LoopsAround()
        {
            var samples = new List<float> { 1, 2, 3, 4 };

            var wavetable = new Wavetable(samples);

            ClassicAssert.AreEqual(1, wavetable.SampleAtPosition(4));
            ClassicAssert.AreEqual(2, wavetable.SampleAtPosition(5));
            ClassicAssert.AreEqual(3, wavetable.SampleAtPosition(6));
            ClassicAssert.AreEqual(4, wavetable.SampleAtPosition(7));
            ClassicAssert.AreEqual(1, wavetable.SampleAtPosition(8));
        }

        [Test]
        public void Size_GivesCorrectSampleCount()
        {
            var samples = new List<float> { 1, 2, 3, 4 };

            var wavetable = new Wavetable(samples);

            ClassicAssert.AreEqual(4, wavetable.Size);
        }
    }
}