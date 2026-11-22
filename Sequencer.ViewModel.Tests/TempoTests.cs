using System;
using Moq;
using NUnit.Framework;
using Sequencer.Midi;

namespace Sequencer.ViewModel.Tests
{
    [TestFixture]
    public class TempoTests
    {
        private Mock<ISequencerClock> clock;
        private ManualTimeProvider time;
        private SequencerViewModel viewModel;

        [SetUp]
        public void SetUp()
        {
            clock = new Mock<ISequencerClock>();
            clock.SetupProperty(x => x.BeatsPerMinute, 120.0);
            time = new ManualTimeProvider();
            viewModel = new SequencerViewModel(clock.Object, new Mock<ITickCalculator>().Object, new Mock<IUiDispatcher>().Object, time);
        }

        [Test]
        public void BeatsPerMinute_StartsAtTheClockTempo()
        {
            Assert.That(viewModel.BeatsPerMinute, Is.EqualTo(120));
        }

        [Test]
        public void SettingBeatsPerMinute_SetsTheClock()
        {
            viewModel.BeatsPerMinute = 95;

            Assert.That(clock.Object.BeatsPerMinute, Is.EqualTo(95));
            Assert.That(viewModel.Information, Does.Contain("95 BPM"));
        }

        [TestCase(5, SequencerViewModel.MinimumBeatsPerMinute)]
        [TestCase(1000, SequencerViewModel.MaximumBeatsPerMinute)]
        public void BeatsPerMinute_IsClamped(int requested, int expected)
        {
            viewModel.BeatsPerMinute = requested;

            Assert.That(viewModel.BeatsPerMinute, Is.EqualTo(expected));
        }

        [Test]
        public void TapTempo_SetsTheTempoFromTheTapInterval()
        {
            // Taps every 0.5 s = 120 BPM; every 0.4 s = 150 BPM.
            Tap(0);
            Tap(400);
            Tap(800);
            Tap(1200);

            Assert.That(viewModel.BeatsPerMinute, Is.EqualTo(150));
        }

        [Test]
        public void TapTempo_AfterAPause_StartsCountingAgain()
        {
            Tap(0);
            Tap(400);
            Assert.That(viewModel.BeatsPerMinute, Is.EqualTo(150));

            Tap(5000);
            Assert.That(viewModel.BeatsPerMinute, Is.EqualTo(150), "A single tap after a pause changes nothing.");

            Tap(6000);
            Assert.That(viewModel.BeatsPerMinute, Is.EqualTo(60));
        }

        private void Tap(long milliseconds)
        {
            time.Milliseconds = milliseconds;
            viewModel.TapTempo.Execute(null);
        }

        private sealed class ManualTimeProvider : TimeProvider
        {
            public long Milliseconds { get; set; }

            public override long TimestampFrequency => 1000;

            public override long GetTimestamp() => Milliseconds;
        }
    }
}
