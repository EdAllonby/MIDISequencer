using System;
using Moq;
using NUnit.Framework;
using Sequencer.View.Command.MousePointCommand;
using Sequencer.View.Command.NotesCommand;
using Sequencer.Visual;
using Sequencer.Visual.Input;

namespace Sequencer.View.Tests.CommandTests.MousePointCommandTests
{
    [TestFixture]
    public class NoteStateCommandFactoryTests
    {
        [Test]
        public void CreateNoteStateCommandWithSelected_CreatesUpdateNoteStateCommand()
        {
            var sequencerNotesMock = new Mock<ISequencerNotes>();
            var keyboardStateProcessorMock = new Mock<IKeyboardStateProcessor>();
            var commandFactory = new NoteStateCommandFactory(sequencerNotesMock.Object, keyboardStateProcessorMock.Object);

            INotesCommand selectedNoteCommand = commandFactory.CreateNoteStateCommand(NoteState.Selected);

            Assert.IsInstanceOf<UpdateNoteStateCommand>(selectedNoteCommand);
        }

        [Test]
        public void CreateNoteStateCommandWithUnknown_ThrowsArgumentOutOfRangeException()
        {
            var sequencerNotesMock = new Mock<ISequencerNotes>();
            var keyboardStateProcessorMock = new Mock<IKeyboardStateProcessor>();
            var commandFactory = new NoteStateCommandFactory(sequencerNotesMock.Object, keyboardStateProcessorMock.Object);

            Assert.Throws<ArgumentOutOfRangeException>(() => commandFactory.CreateNoteStateCommand((NoteState) (-1)));
        }

        [Test]
        public void CreateNoteStateCommandWithUnselected_CreatesUpdateNoteStateCommand()
        {
            var sequencerNotesMock = new Mock<ISequencerNotes>();
            var keyboardStateProcessorMock = new Mock<IKeyboardStateProcessor>();
            var commandFactory = new NoteStateCommandFactory(sequencerNotesMock.Object, keyboardStateProcessorMock.Object);

            INotesCommand selectedNoteCommand = commandFactory.CreateNoteStateCommand(NoteState.Unselected);

            Assert.IsInstanceOf<UpdateNoteStateCommand>(selectedNoteCommand);
        }
    }
}