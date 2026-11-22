using NUnit.Framework;
using Sequencer.Audio.Calculator;

namespace Sequencer.Audio.Tests
{
    [TestFixture]
    public class WavetableCalculatorTests
    {
        private const double Tolerance = 0.001;

        [TestCase(0.0, 0.0)]
        [TestCase(0.25, 1.0)]
        [TestCase(0.5, 0.0)]
        [TestCase(0.75, -1.0)]
        [TestCase(0.1, 0.587785)]
        public void CalculateForPhase_MatchesTheSourceWaveform(double phase, double expected)
        {
            var calculator = new WavetableCalculator(new SineWaveCalculator());

            Assert.That(calculator.CalculateForPhase(phase), Is.EqualTo(expected).Within(Tolerance));
        }

        [Test]
        public void CalculateForPhase_InterpolatesAcrossTheEndOfTheCycle()
        {
            var calculator = new WavetableCalculator(new SineWaveCalculator());

            // Just before the end of the cycle the value is just below zero, heading back to 0 at phase 1.
            Assert.That(calculator.CalculateForPhase(0.99999), Is.EqualTo(0.0).Within(Tolerance));
        }

        [Test]
        public void SawtoothWaveCalculator_RampsFromNegativeToPositiveAcrossTheCycle()
        {
            var calculator = new SawtoothWaveCalculator(19);

            Assert.That(calculator.CalculateForPhase(0.1), Is.GreaterThan(0));
            Assert.That(calculator.CalculateForPhase(0.9), Is.LessThan(0));
        }
    }
}
