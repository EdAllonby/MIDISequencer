using System.Windows.Input;
using Moq;
using NUnit.Framework;
using Sequencer.Midi;

namespace Sequencer.ViewModel.Tests
{
    [TestFixture]
    public class SequencerViewModelTests
    {
        [Test]
        public void ExecutingPlayCommand_SetsPlayState_ToTrue()
        {
            var viewModel = new SequencerViewModel(new Mock<ISequencerClock>().Object, new Mock<ITickCalculator>().Object, new Mock<IWpfDispatcher>().Object)
                { SequencerPlayState = PlayState.Stop };

            ICommand playCommand = viewModel.PlaySequencer;

            playCommand.Execute(null);

            Assert.AreEqual(PlayState.Play, viewModel.SequencerPlayState);
        }

        [Test]
        public void ExecutingToggleSerquencerCommand_WhenPaused_SetsPlayStateToTrue()
        {
            var viewModel = new SequencerViewModel(new Mock<ISequencerClock>().Object, new Mock<ITickCalculator>().Object, new Mock<IWpfDispatcher>().Object)
                { SequencerPlayState = PlayState.Pause };

            ICommand toggleSequencerCommand = viewModel.ToggleSequencer;

            toggleSequencerCommand.Execute(null);

            Assert.AreEqual(PlayState.Play, viewModel.SequencerPlayState);
        }

        [Test]
        public void ExecutingToggleSerquencerCommand_WhenPaused_SetsPlayStateToPause()
        {
            var viewModel = new SequencerViewModel(new Mock<ISequencerClock>().Object, new Mock<ITickCalculator>().Object, new Mock<IWpfDispatcher>().Object)
                { SequencerPlayState = PlayState.Play };

            ICommand toggleSequencerCommand = viewModel.ToggleSequencer;

            toggleSequencerCommand.Execute(null);

            Assert.AreEqual(PlayState.Pause, viewModel.SequencerPlayState);
        }

        [Test]
        public void ExecutingStopCommand_SetsPlayState_ToFalse()
        {
            var viewModel = new SequencerViewModel(new Mock<ISequencerClock>().Object, new Mock<ITickCalculator>().Object, new Mock<IWpfDispatcher>().Object)
                { SequencerPlayState = PlayState.Play };

            ICommand stopCommand = viewModel.StopSequencer;

            stopCommand.Execute(null);

            Assert.AreEqual(PlayState.Stop, viewModel.SequencerPlayState);
        }

        [Test]
        public void Information_ShowsNoteActionAndPlayState()
        {
            var viewModel = new SequencerViewModel(new Mock<ISequencerClock>().Object, new Mock<ITickCalculator>().Object, new Mock<IWpfDispatcher>().Object)
            {
                SequencerPlayState = PlayState.Stop,
                NoteAction = NoteAction.Select
            };

            viewModel.SequencerPlayState = PlayState.Pause;

            const string expectedInformation = "Note Action Select, Pause";

            Assert.AreEqual(expectedInformation, viewModel.Information);
        }

        [Test]
        public void SequencerInStartState_CanExecuteStop()
        {
            var viewModel = new SequencerViewModel(new Mock<ISequencerClock>().Object, new Mock<ITickCalculator>().Object, new Mock<IWpfDispatcher>().Object)
                { SequencerPlayState = PlayState.Play };

            ICommand stopCommand = viewModel.StopSequencer;

            Assert.IsTrue(stopCommand.CanExecute(null));
        }

        [Test]
        public void SequencerInStartState_CannotExecuteStart()
        {
            var viewModel = new SequencerViewModel(new Mock<ISequencerClock>().Object, new Mock<ITickCalculator>().Object, new Mock<IWpfDispatcher>().Object)
                { SequencerPlayState = PlayState.Play };

            ICommand playCommand = viewModel.PlaySequencer;

            Assert.IsFalse(playCommand.CanExecute(null));
        }

        [Test]
        public void SequencerInStopState_CanExecuteStart()
        {
            var viewModel = new SequencerViewModel(new Mock<ISequencerClock>().Object, new Mock<ITickCalculator>().Object, new Mock<IWpfDispatcher>().Object)
                { SequencerPlayState = PlayState.Stop };

            ICommand playCommand = viewModel.PlaySequencer;

            Assert.IsTrue(playCommand.CanExecute(null));
        }

        [Test]
        public void SequencerInStopState_CannotExecuteStop()
        {
            var viewModel = new SequencerViewModel(new Mock<ISequencerClock>().Object, new Mock<ITickCalculator>().Object, new Mock<IWpfDispatcher>().Object)
                { SequencerPlayState = PlayState.Stop };

            ICommand stopCommand = viewModel.StopSequencer;

            Assert.IsFalse(stopCommand.CanExecute(null));
        }

        [Test]
        public void SequencerPause_ShouldPauseClock()
        {
            var mockClock = new Mock<ISequencerClock>();

            var viewModel = new SequencerViewModel(mockClock.Object, new Mock<ITickCalculator>().Object, new Mock<IWpfDispatcher>().Object)
                { SequencerPlayState = PlayState.Play };

            ICommand pauseCommand = viewModel.PauseSequencer;
            pauseCommand.Execute(null);

            mockClock.Verify(x => x.Pause());
        }

        [Test]
        public void SequencerStart_ShouldStartClock()
        {
            var mockClock = new Mock<ISequencerClock>();

            var viewModel = new SequencerViewModel(mockClock.Object, new Mock<ITickCalculator>().Object, new Mock<IWpfDispatcher>().Object)
                { SequencerPlayState = PlayState.Stop };

            ICommand playCommand = viewModel.PlaySequencer;
            playCommand.Execute(null);

            mockClock.Verify(x => x.Start());
        }

        [Test]
        public void SequencerStop_ShouldStopClock()
        {
            var mockClock = new Mock<ISequencerClock>();

            var viewModel = new SequencerViewModel(mockClock.Object, new Mock<ITickCalculator>().Object, new Mock<IWpfDispatcher>().Object)
                { SequencerPlayState = PlayState.Play };

            ICommand stopCommand = viewModel.StopSequencer;
            stopCommand.Execute(null);

            mockClock.Verify(x => x.Stop());
        }
    }
}