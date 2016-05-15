using NUnit.Framework;
using Sequencer.Domain;

namespace Sequencer.Tests
{
    [TestFixture]
    internal class PitchFrequencyTests
    {
        private readonly FrequencyCalculator calculator = new FrequencyCalculator(new Pitch(Note.A, 4), 440);

        // Realistically, we're not going to hear a difference between a frequency of 330Hz and 330.01Hz.
        private const double Tolerance = 0.01;

        private static readonly object[] PitchFrequencyCases =
        {
            new object[] {new Pitch(Note.C, 0), 16.35},
            new object[] {new Pitch(Note.D, 2), 73.42},
            new object[] {new Pitch(Note.B, 8), 7902.13},
            new object[] {new Pitch(Note.A, 4), 440},
            new object[] {new Pitch(Note.A, 3), 220},
            new object[] {new Pitch(Note.B, 3), 246.94},
            new object[] {new Pitch(Note.C, 4), 261.63},
            new object[] {new Pitch(Note.F, 6), 1396.91},
            new object[] {new Pitch(Note.DSharp, 8), 4978.03}
        };


        [Test, TestCaseSource(nameof(PitchFrequencyCases))]
        public void FrequencyFromPitchTest(Pitch pitch, double expectedFrequency)
        {
            double actualFrequency = calculator.PitchFrequency(pitch);
            Assert.That(expectedFrequency, Is.EqualTo(actualFrequency).Within(Tolerance));
        }
    }
}