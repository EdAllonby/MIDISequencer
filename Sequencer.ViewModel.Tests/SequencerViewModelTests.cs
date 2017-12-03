using System.Windows.Input;
using NUnit.Framework;

namespace Sequencer.ViewModel.Tests
{
    [TestFixture]
    public class SequencerViewModelTests
    {
        [Test]
        public void ExecutingPlayCommand_SetsPlayState_ToTrue()
        {
            var viewModel = new SequencerViewModel { SequencerPlaying = false };

            ICommand playCommand = viewModel.PlaySequencer;

            playCommand.Execute(null);

            Assert.IsTrue(viewModel.SequencerPlaying);
        }

        [Test]
        public void ExecutingStopCommand_SetsPlayState_ToFalse()
        {
            var viewModel = new SequencerViewModel { SequencerPlaying = true };

            ICommand stopCommand = viewModel.StopSequencer;

            stopCommand.Execute(null);

            Assert.IsFalse(viewModel.SequencerPlaying);
        }

        [Test]
        public void SequencerInStopState_CannotExecuteStop()
        {
            var viewModel = new SequencerViewModel { SequencerPlaying = false };


            ICommand stopCommand = viewModel.StopSequencer;

            Assert.IsFalse(stopCommand.CanExecute(null));
        }

        [Test]
        public void SequencerInStartState_CannotExecuteStart()
        {
            var viewModel = new SequencerViewModel { SequencerPlaying = true };


            ICommand playCommand = viewModel.PlaySequencer;

            Assert.IsFalse(playCommand.CanExecute(null));
        }

        [Test]
        public void SequencerInStopState_CanExecuteStart()
        {
            var viewModel = new SequencerViewModel { SequencerPlaying = false };


            ICommand playCommand = viewModel.PlaySequencer;

            Assert.IsTrue(playCommand.CanExecute(null));
        }

        [Test]
        public void SequencerInStartState_CanExecuteStop()
        {
            var viewModel = new SequencerViewModel { SequencerPlaying = true };


            ICommand stopCommand = viewModel.StopSequencer;

            Assert.IsTrue(stopCommand.CanExecute(null));
        }
    }
}