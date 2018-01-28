using System;
using Moq;
using NUnit.Framework;
using Sequencer.Domain;
using Sequencer.Domain.Settings;

namespace Sequencer.Midi.Tests
{
    [TestFixture]
    public class SequencerClockTests
    {
        private static SequencerClock GetSequencerClock()
        {
            Mock<IMusicalSettings> mockMusicalSettings = MockMusicalSettings();

            var mockInternalClock = new Mock<IInternalClock>();
            mockInternalClock.Setup(x => x.Tempo).Returns(500000);

            return new SequencerClock(mockMusicalSettings.Object, mockInternalClock.Object);
        }

        private static Mock<IMusicalSettings> MockMusicalSettings()
        {
            var mockMusicalSettings = new Mock<IMusicalSettings>();
            mockMusicalSettings.Setup(x => x.TicksPerQuarterNote).Returns(120);
            mockMusicalSettings.Setup(x => x.TotalMeasures).Returns(4);
            mockMusicalSettings.Setup(x => x.TimeSignature).Returns(TimeSignature.FourFour);
            return mockMusicalSettings;
        }

        [Test]
        public void Continue_CallsContinue()
        {
            Mock<IMusicalSettings> mockSettings = MockMusicalSettings();
            var mockInternalClock = new Mock<IInternalClock>();

            var clock = new SequencerClock(mockSettings.Object, mockInternalClock.Object);

            clock.Continue();

            mockInternalClock.Verify(x => x.Continue(), Times.Once);
        }

        [Test]
        public void DefaultTempo_Is120BeatsPerMinute()
        {
            SequencerClock sequencerClock = GetSequencerClock();

            Assert.AreEqual(120, sequencerClock.BeatsPerMinute);
        }

        [Test]
        public void GetTicks_GetsInternalClockTicks()
        {
            Mock<IMusicalSettings> mockSettings = MockMusicalSettings();
            var mockInternalClock = new Mock<IInternalClock>();

            const int expectedTicks = 100;
            mockInternalClock.Setup(x => x.Ticks).Returns(expectedTicks);

            var clock = new SequencerClock(mockSettings.Object, mockInternalClock.Object);

            Assert.AreEqual(expectedTicks, clock.Ticks);
        }

        [Test]
        public void OnTick_DoesNotResetsTickCount_WhenNotPassingTotalTicks()
        {
            Mock<IMusicalSettings> mockSettings = MockMusicalSettings();
            var mockInternalClock = new Mock<IInternalClock>();
            mockInternalClock.Setup(x => x.Ppqn).Returns(120);
            mockInternalClock.Setup(x => x.Ticks).Returns(7679); // 2 ticks before loop.

            IInternalClock internalClock = mockInternalClock.Object;

            var unused = new SequencerClock(mockSettings.Object, internalClock);

            mockInternalClock.Raise(x => x.Tick += null, EventArgs.Empty);

            mockInternalClock.VerifySet(x => x.Ticks = It.IsAny<int>(), Times.Never);
            mockInternalClock.Verify(x => x.Start(), Times.Never);
        }

        [Test]
        public void OnTick_ResetsTickCount_WhenPassingTotalTicks()
        {
            Mock<IMusicalSettings> mockSettings = MockMusicalSettings();
            var mockInternalClock = new Mock<IInternalClock>();
            mockInternalClock.Setup(x => x.Ppqn).Returns(120);
            mockInternalClock.Setup(x => x.Ticks).Returns(7680); // last tick before loop.

            IInternalClock internalClock = mockInternalClock.Object;

            var unused = new SequencerClock(mockSettings.Object, internalClock);

            mockInternalClock.Raise(x => x.Tick += null, EventArgs.Empty);

            mockInternalClock.VerifySet(x => x.Ticks = 0, Times.Once);
            mockInternalClock.Verify(x => x.Start(), Times.Once);
        }

        [Test]
        public void Pause_KeepsTicks()
        {
            Mock<IMusicalSettings> mockSettings = MockMusicalSettings();
            var mockInternalClock = new Mock<IInternalClock>();

            var clock = new SequencerClock(mockSettings.Object, mockInternalClock.Object);

            clock.Pause();

            mockInternalClock.Verify(x => x.Stop(), Times.Once);
            mockInternalClock.VerifySet(x => x.Ticks = It.IsAny<int>(), Times.Never);
        }

        [Test]
        public void Start_CallsContinue()
        {
            Mock<IMusicalSettings> mockSettings = MockMusicalSettings();
            var mockInternalClock = new Mock<IInternalClock>();

            var clock = new SequencerClock(mockSettings.Object, mockInternalClock.Object);

            clock.Start();

            mockInternalClock.Verify(x => x.Continue(), Times.Once);
        }

        [Test]
        public void Stop_SetsTicksBackToZero()
        {
            Mock<IMusicalSettings> mockSettings = MockMusicalSettings();
            var mockInternalClock = new Mock<IInternalClock>();

            var clock = new SequencerClock(mockSettings.Object, mockInternalClock.Object);

            clock.Stop();

            mockInternalClock.Verify(x => x.Stop(), Times.Once);
            mockInternalClock.VerifySet(x => x.Ticks = 0, Times.Once);
        }
    }
}