using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using Sequencer.Domain.Settings;

namespace Sequencer.Domain.Tests
{
    [TestFixture]
    public class SynthPresetTests
    {
        private static IEnumerable<SynthPreset> Presets => SynthPresets.All;

        private static IEnumerable<PropertyInfo> Settable => typeof(SynthSettings).GetProperties().Where(p => p.CanWrite);

        [Test]
        public void Init_IsTheDefaultPatch()
        {
            SynthSettings init = SynthPresets.All.Single(p => p.Name == "Init").CreatePatch();
            var defaults = new SynthSettings();

            foreach (PropertyInfo property in Settable)
            {
                Assert.That(property.GetValue(init), Is.EqualTo(property.GetValue(defaults)), property.Name);
            }
        }

        [TestCaseSource(nameof(Presets))]
        public void EveryPreset_ChangesSomething_UnlessItIsInit(SynthPreset preset)
        {
            SynthSettings patch = preset.CreatePatch();
            var defaults = new SynthSettings();
            bool differs = Settable.Any(property => !Equals(property.GetValue(patch), property.GetValue(defaults)));

            Assert.That(differs, Is.EqualTo(preset.Name != "Init"));
        }

        [Test]
        public void CopyFrom_CopiesEverySetting()
        {
            // Every setting differs from the default in this patch, so a forgotten one would stay at its default.
            var source = new SynthSettings
            {
                Waveform = Waveform.Sine, Oscillator2Waveform = Waveform.Square, Oscillator2Semitones = 7, Oscillator2Detune = 9,
                OscillatorMix = 0.3, NoiseLevel = 0.2, FilterType = FilterType.BandPass, Cutoff = 900, Resonance = 0.4,
                FilterEnvelopeAmount = 1.5, KeyTracking = 0.6, FilterAttackSeconds = 0.2, FilterDecaySeconds = 0.4,
                FilterSustainLevel = 0.3, FilterReleaseSeconds = 0.9, AttackSeconds = 0.3, DecaySeconds = 0.6, SustainLevel = 0.2,
                ReleaseSeconds = 1.1, LfoRate = 7, LfoDepth = 0.5, LfoTarget = LfoTarget.Volume, Volume = 0.4
            };
            var copy = new SynthSettings();

            copy.CopyFrom(source);

            foreach (PropertyInfo property in Settable)
            {
                Assert.That(property.GetValue(copy), Is.EqualTo(property.GetValue(source)), property.Name);
            }
        }
    }
}
