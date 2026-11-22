using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Sequencer.Domain.Settings;

namespace Sequencer.ViewModel.Tests
{
    [TestFixture]
    public class SubtractiveSynthViewModelTests
    {
        private SynthSettings settings;
        private SynthViewModel viewModel;

        [SetUp]
        public void SetUp()
        {
            settings = new SynthSettings();
            viewModel = new SynthViewModel(settings);
        }

        [TestCase(0, 20)]
        [TestCase(1000, 18000)]
        public void CutoffPosition_SpansTheRangeLogarithmically(int position, double hertz)
        {
            viewModel.CutoffPosition = position;

            Assert.That(settings.Cutoff, Is.EqualTo(hertz).Within(0.5));
        }

        [Test]
        public void CutoffPosition_GivesEachOctaveEqualTravel()
        {
            viewModel.CutoffPosition = 500;
            double middle = settings.Cutoff;

            // Halfway along a 20 Hz - 18 kHz log scale is their geometric mean, about 600 Hz.
            Assert.That(middle, Is.EqualTo(600).Within(5));
            Assert.That(viewModel.CutoffText, Is.EqualTo("600 Hz"));
        }

        [Test]
        public void CutoffText_UsesKilohertzAboveOneThousand()
        {
            settings.Cutoff = 2500;

            Assert.That(viewModel.CutoffText, Is.EqualTo("2.5 kHz"));
        }

        [Test]
        public void FilterEnvelopeAmount_IsShownInSignedOctaves()
        {
            viewModel.FilterEnvelopeAmountTenths = 25;
            Assert.That(viewModel.FilterEnvelopeAmountText, Is.EqualTo("+2.5 oct"));

            viewModel.FilterEnvelopeAmountTenths = -10;
            Assert.That(viewModel.FilterEnvelopeAmountText, Is.EqualTo("-1.0 oct"));
        }

        [Test]
        public void Toggles_SelectOneOptionEach()
        {
            viewModel.IsOscillator2Square = true;
            viewModel.IsHighPass = true;
            viewModel.IsLfoOnPitch = true;

            Assert.That(settings.Oscillator2Waveform, Is.EqualTo(Waveform.Square));
            Assert.That(viewModel.IsOscillator2Sawtooth, Is.False);
            Assert.That(settings.FilterType, Is.EqualTo(FilterType.HighPass));
            Assert.That(viewModel.IsLowPass, Is.False);
            Assert.That(settings.LfoTarget, Is.EqualTo(LfoTarget.Pitch));
        }

        [Test]
        public void ApplyPreset_ReplacesThePatch_AndRefreshesEverything()
        {
            var changed = new List<string>();
            viewModel.PropertyChanged += (sender, args) => changed.Add(args.PropertyName);

            viewModel.ApplyPreset(SynthPresets.All.Single(p => p.Name == "Acid Bass"));

            Assert.That(viewModel.ResonancePercent, Is.EqualTo(80));
            Assert.That(viewModel.FilterEnvelopeAmountText, Is.EqualTo("+3.5 oct"));
            Assert.That(changed, Does.Contain(string.Empty), "One notification refreshes every control.");
        }

        [Test]
        public void Settings_CanBeRetargetedToAnotherTrack()
        {
            var other = new SynthSettings { Resonance = 0.4 };

            viewModel.Settings = other;

            Assert.That(viewModel.ResonancePercent, Is.EqualTo(40));
        }
    }
}
