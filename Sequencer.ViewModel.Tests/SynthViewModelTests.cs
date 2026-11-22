using NUnit.Framework;
using Sequencer.Domain.Settings;

namespace Sequencer.ViewModel.Tests
{
    [TestFixture]
    public class SynthViewModelTests
    {
        private SynthSettings settings;
        private SynthViewModel viewModel;

        [SetUp]
        public void SetUp()
        {
            settings = new SynthSettings();
            viewModel = new SynthViewModel(settings);
        }

        [Test]
        public void ShowsTheSettingsInMillisecondsAndPercent()
        {
            Assert.That(viewModel.AttackMilliseconds, Is.EqualTo(5));
            Assert.That(viewModel.DecayMilliseconds, Is.EqualTo(100));
            Assert.That(viewModel.SustainPercent, Is.EqualTo(70));
            Assert.That(viewModel.ReleaseMilliseconds, Is.EqualTo(200));
            Assert.That(viewModel.VolumePercent, Is.EqualTo(100));
        }

        [Test]
        public void Changes_AreWrittenToTheSettings()
        {
            viewModel.Waveform = Waveform.Square;
            viewModel.AttackMilliseconds = 250;
            viewModel.SustainPercent = 40;
            viewModel.VolumePercent = 60;

            Assert.That(settings.Waveform, Is.EqualTo(Waveform.Square));
            Assert.That(settings.AttackSeconds, Is.EqualTo(0.25));
            Assert.That(settings.SustainLevel, Is.EqualTo(0.4));
            Assert.That(settings.MasterVolume, Is.EqualTo(0.6));
        }

        [Test]
        public void OutOfRangeValues_AreClamped()
        {
            viewModel.AttackMilliseconds = 0;
            viewModel.SustainPercent = 150;

            Assert.That(viewModel.AttackMilliseconds, Is.EqualTo(1), "An envelope stage needs at least 1 ms.");
            Assert.That(viewModel.SustainPercent, Is.EqualTo(100));
        }

        [Test]
        public void Changes_RaisePropertyChanged()
        {
            string changed = null;
            viewModel.PropertyChanged += (sender, args) => changed = args.PropertyName;

            viewModel.ReleaseMilliseconds = 900;

            Assert.That(changed, Is.EqualTo(nameof(SynthViewModel.ReleaseMilliseconds)));
        }

        [Test]
        public void Waveforms_ListsEveryWaveform()
        {
            Assert.That(viewModel.Waveforms, Is.EquivalentTo(new[] { Waveform.Sawtooth, Waveform.Square, Waveform.Triangle, Waveform.Sine }));
        }
    }
}
