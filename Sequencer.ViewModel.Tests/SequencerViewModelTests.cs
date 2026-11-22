using System;
using System.Windows.Input;
using Moq;
using NUnit.Framework;
using NUnit.Framework.Legacy;
using Sequencer.Domain;
using Sequencer.Midi;

namespace Sequencer.ViewModel.Tests
{
    [TestFixture]
    public class SequencerViewModelTests
    {
        [Test]
        public void ExecutingPlayCommand_SetsPlayState_ToTrue()
        {
            var viewModel = new SequencerViewModel(new Mock<ISequencerClock>().Object, new Mock<ITickCalculator>().Object, new Mock<IUiDispatcher>().Object)
                { SequencerPlayState = PlayState.Stop };

            ICommand playCommand = viewModel.PlaySequencer;

            playCommand.Execute(null);

            ClassicAssert.AreEqual(PlayState.Play, viewModel.SequencerPlayState);
        }

        [Test]
        public void ExecutingStopCommand_SetsPlayState_ToFalse()
        {
            var viewModel = new SequencerViewModel(new Mock<ISequencerClock>().Object, new Mock<ITickCalculator>().Object, new Mock<IUiDispatcher>().Object)
                { SequencerPlayState = PlayState.Play };

            ICommand stopCommand = viewModel.StopSequencer;

            stopCommand.Execute(null);

            ClassicAssert.AreEqual(PlayState.Stop, viewModel.SequencerPlayState);
        }

        [Test]
        public void ExecutingToggleSerquencerCommand_WhenPaused_SetsPlayStateToPause()
        {
            var viewModel = new SequencerViewModel(new Mock<ISequencerClock>().Object, new Mock<ITickCalculator>().Object, new Mock<IUiDispatcher>().Object)
                { SequencerPlayState = PlayState.Play };

            ICommand toggleSequencerCommand = viewModel.ToggleSequencer;

            toggleSequencerCommand.Execute(null);

            ClassicAssert.AreEqual(PlayState.Pause, viewModel.SequencerPlayState);
        }

        [Test]
        public void ExecutingToggleSerquencerCommand_WhenPaused_SetsPlayStateToTrue()
        {
            var viewModel = new SequencerViewModel(new Mock<ISequencerClock>().Object, new Mock<ITickCalculator>().Object, new Mock<IUiDispatcher>().Object)
                { SequencerPlayState = PlayState.Pause };

            ICommand toggleSequencerCommand = viewModel.ToggleSequencer;

            toggleSequencerCommand.Execute(null);

            ClassicAssert.AreEqual(PlayState.Play, viewModel.SequencerPlayState);
        }

        [Test]
        public void Information_ShowsNoteActionAndPlayState()
        {
            var viewModel = new SequencerViewModel(new Mock<ISequencerClock>().Object, new Mock<ITickCalculator>().Object, new Mock<IUiDispatcher>().Object)
            {
                SequencerPlayState = PlayState.Stop,
                NoteAction = NoteAction.Select
            };

            viewModel.SequencerPlayState = PlayState.Pause;

            const string expectedInformation = "Note Action Select, Pause, 120 BPM";

            ClassicAssert.AreEqual(expectedInformation, viewModel.Information);
        }

        [Test]
        public void Sequencer_OnTick_CallsDispatcher()
        {
            const int currentTick = 8;

            var mockClock = new Mock<ISequencerClock>();
            mockClock.Setup(x => x.TicksPerQuarterNote).Returns(16);

            var mockTickCalculator = new Mock<ITickCalculator>();
            mockTickCalculator.Setup(x => x.CalculatePositionFromTick(currentTick)).Returns(It.IsAny<IPosition>());

            var mockDispatcher = new Mock<IUiDispatcher>();

            var unused = new SequencerViewModel(mockClock.Object, mockTickCalculator.Object, mockDispatcher.Object);

            mockClock.Raise(x => x.Tick += null, new TickEventArgs(currentTick));

            mockDispatcher.Verify(x => x.Dispatch(It.IsAny<Action>()), Times.Once);
        }

        [Test]
        public void Sequencer_OnTick_SetsCorrectCurrentPosition()
        {
            const int currentTick = 8;
            var expectedPosition = new Position(1, 2, 3, 4);

            var mockClock = new Mock<ISequencerClock>();
            mockClock.Setup(x => x.TicksPerQuarterNote).Returns(16);

            var mockTickCalculator = new Mock<ITickCalculator>();
            mockTickCalculator.Setup(x => x.CalculatePositionFromTick(currentTick)).Returns(expectedPosition);

            var dispatcher = new Mock<IUiDispatcher>();
            dispatcher.Setup(x => x.Dispatch(It.IsAny<Action>())).Callback<Action>(dispatcherCallback => dispatcherCallback());

            var viewModel = new SequencerViewModel(mockClock.Object, mockTickCalculator.Object, dispatcher.Object);

            mockClock.Raise(x => x.Tick += null, new TickEventArgs(currentTick));

            ClassicAssert.AreEqual(viewModel.CurrentPosition, expectedPosition);
        }

        [Test]
        public void Sequencer_OnTickInBetweenResolution_DoesNotCallDispatcher()
        {
            const int currentTick = 7;

            var mockClock = new Mock<ISequencerClock>();
            mockClock.Setup(x => x.TicksPerQuarterNote).Returns(16);

            var mockTickCalculator = new Mock<ITickCalculator>();
            var mockDispatcher = new Mock<IUiDispatcher>();

            var unused = new SequencerViewModel(mockClock.Object, mockTickCalculator.Object, mockDispatcher.Object);

            mockClock.Raise(x => x.Tick += null, new TickEventArgs(currentTick));

            mockDispatcher.Verify(x => x.Dispatch(It.IsAny<Action>()), Times.Never);
        }

        [Test]
        public void SequencerInStartState_CanExecuteStop()
        {
            var viewModel = new SequencerViewModel(new Mock<ISequencerClock>().Object, new Mock<ITickCalculator>().Object, new Mock<IUiDispatcher>().Object)
                { SequencerPlayState = PlayState.Play };

            ICommand stopCommand = viewModel.StopSequencer;

            ClassicAssert.IsTrue(stopCommand.CanExecute(null));
        }

        [Test]
        public void SequencerInStartState_CannotExecuteStart()
        {
            var viewModel = new SequencerViewModel(new Mock<ISequencerClock>().Object, new Mock<ITickCalculator>().Object, new Mock<IUiDispatcher>().Object)
                { SequencerPlayState = PlayState.Play };

            ICommand playCommand = viewModel.PlaySequencer;

            ClassicAssert.IsFalse(playCommand.CanExecute(null));
        }

        [Test]
        public void SequencerInStopState_CanExecuteStart()
        {
            var viewModel = new SequencerViewModel(new Mock<ISequencerClock>().Object, new Mock<ITickCalculator>().Object, new Mock<IUiDispatcher>().Object)
                { SequencerPlayState = PlayState.Stop };

            ICommand playCommand = viewModel.PlaySequencer;

            ClassicAssert.IsTrue(playCommand.CanExecute(null));
        }

        [Test]
        public void SequencerInStopState_CannotExecuteStop()
        {
            var viewModel = new SequencerViewModel(new Mock<ISequencerClock>().Object, new Mock<ITickCalculator>().Object, new Mock<IUiDispatcher>().Object)
                { SequencerPlayState = PlayState.Stop };

            ICommand stopCommand = viewModel.StopSequencer;

            ClassicAssert.IsFalse(stopCommand.CanExecute(null));
        }

        [Test]
        public void SequencerPause_ShouldPauseClock()
        {
            var mockClock = new Mock<ISequencerClock>();

            var viewModel = new SequencerViewModel(mockClock.Object, new Mock<ITickCalculator>().Object, new Mock<IUiDispatcher>().Object)
                { SequencerPlayState = PlayState.Play };

            ICommand pauseCommand = viewModel.PauseSequencer;
            pauseCommand.Execute(null);

            mockClock.Verify(x => x.Pause());
        }

        [Test]
        public void SequencerStart_ShouldStartClock()
        {
            var mockClock = new Mock<ISequencerClock>();

            var viewModel = new SequencerViewModel(mockClock.Object, new Mock<ITickCalculator>().Object, new Mock<IUiDispatcher>().Object)
                { SequencerPlayState = PlayState.Stop };

            ICommand playCommand = viewModel.PlaySequencer;
            playCommand.Execute(null);

            mockClock.Verify(x => x.Start());
        }

        [Test]
        public void SequencerStop_ShouldStopClock()
        {
            var mockClock = new Mock<ISequencerClock>();

            var viewModel = new SequencerViewModel(mockClock.Object, new Mock<ITickCalculator>().Object, new Mock<IUiDispatcher>().Object)
                { SequencerPlayState = PlayState.Play };

            ICommand stopCommand = viewModel.StopSequencer;
            stopCommand.Execute(null);

            mockClock.Verify(x => x.Stop());
        }
    }
}