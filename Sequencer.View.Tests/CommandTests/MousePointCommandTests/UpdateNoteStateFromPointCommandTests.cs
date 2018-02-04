using System.Collections.Generic;
using System.Windows.Input;
using Moq;
using NUnit.Framework;
using Sequencer.View.Command.MousePointCommand;
using Sequencer.View.Command.NotesCommand;
using Sequencer.Visual;
using Sequencer.Visual.Input;

namespace Sequencer.View.Tests.CommandTests.MousePointCommandTests
{
    [TestFixture]
    public class UpdateNoteStateFromPointCommandTests
    {
        private static Mock<IVisualNote> CreateVisualNoteMock(NoteState noteState)
        {
            var visualNoteMock = new Mock<IVisualNote>();
            visualNoteMock.Setup(x => x.NoteState).Returns(noteState);
            return visualNoteMock;
        }

        private static Mock<IKeyboardStateProcessor> CreateKeyboardStateProcessorMock(bool isLeftCtrlPressed)
        {
            var keyboardStateProcessorMock = new Mock<IKeyboardStateProcessor>();
            keyboardStateProcessorMock.Setup(x => x.IsKeyDown(Key.LeftCtrl)).Returns(isLeftCtrlPressed);
            return keyboardStateProcessorMock;
        }

        [Test]
        public void NoNotesUnderPoint_WithLeftCtrlNotPressed_UnselectsAllSelectedNotes()
        {
            var sequencerNotesMock = new Mock<ISequencerNotes>();

            // ReSharper disable once CollectionNeverUpdated.Local
            var selectedNotes = new List<IVisualNote>();
            sequencerNotesMock.Setup(x => x.SelectedNotes).Returns(selectedNotes);

            var mouseOperatorMock = new Mock<IMouseOperator>();
            mouseOperatorMock.Setup(x => x.CanModifyNote).Returns(true);

            Mock<IKeyboardStateProcessor> keyboardStateProcessorMock = CreateKeyboardStateProcessorMock(false);

            var mousePoint = It.IsAny<IMousePoint>();

            var sequencerDimensionsCalculatorMock = new Mock<ISequencerDimensionsCalculator>();

            // No notes under point
            sequencerDimensionsCalculatorMock
                .Setup(x => x.FindNoteFromPoint(sequencerNotesMock.Object, mousePoint))
                .Returns<ISequencerNotes>(null);

            var noteStateCommandFactoryMock = new Mock<INoteStateCommandFactory>();

            var unselectCommandMock = new Mock<INotesCommand>();
            noteStateCommandFactoryMock.Setup(x => x.CreateNoteStateCommand(NoteState.Unselected)).Returns(unselectCommandMock.Object);

            var command = new UpdateNoteStateFromPointCommand(sequencerNotesMock.Object, mouseOperatorMock.Object,
                keyboardStateProcessorMock.Object, sequencerDimensionsCalculatorMock.Object, noteStateCommandFactoryMock.Object);

            command.Execute(mousePoint);

            unselectCommandMock.Verify(x => x.Execute(selectedNotes));
        }

        [Test]
        public void NoteSelectedUnderPoint_WithLeftCtrlPressed_UnselectsNote()
        {
            Mock<IVisualNote> visualNoteMock = CreateVisualNoteMock(NoteState.Selected);

            var sequencerNotesMock = new Mock<ISequencerNotes>();
            var mouseOperatorMock = new Mock<IMouseOperator>();
            mouseOperatorMock.Setup(x => x.CanModifyNote).Returns(true);

            Mock<IKeyboardStateProcessor> keyboardStateProcessorMock = CreateKeyboardStateProcessorMock(true);

            var mousePoint = It.IsAny<IMousePoint>();

            var sequencerDimensionsCalculatorMock = new Mock<ISequencerDimensionsCalculator>();

            sequencerDimensionsCalculatorMock
                .Setup(x => x.FindNoteFromPoint(sequencerNotesMock.Object, mousePoint))
                .Returns(visualNoteMock.Object);

            var noteStateCommandFactoryMock = new Mock<INoteStateCommandFactory>();

            var unselectCommandMock = new Mock<INotesCommand>();
            noteStateCommandFactoryMock.Setup(x => x.CreateNoteStateCommand(NoteState.Unselected)).Returns(unselectCommandMock.Object);

            var command = new UpdateNoteStateFromPointCommand(sequencerNotesMock.Object, mouseOperatorMock.Object,
                keyboardStateProcessorMock.Object, sequencerDimensionsCalculatorMock.Object, noteStateCommandFactoryMock.Object);

            command.Execute(mousePoint);

            unselectCommandMock.Verify(x => x.Execute(new List<IVisualNote> { visualNoteMock.Object }));
        }

        [Test]
        public void NoteUnselectedUnderPoint_WithLeftCtrlPressed_SelectsNote()
        {
            Mock<IVisualNote> visualNoteMock = CreateVisualNoteMock(NoteState.Unselected);

            var sequencerNotesMock = new Mock<ISequencerNotes>();
            var mouseOperatorMock = new Mock<IMouseOperator>();
            mouseOperatorMock.Setup(x => x.CanModifyNote).Returns(true);

            Mock<IKeyboardStateProcessor> keyboardStateProcessorMock = CreateKeyboardStateProcessorMock(true);

            var mousePoint = It.IsAny<IMousePoint>();

            var sequencerDimensionsCalculatorMock = new Mock<ISequencerDimensionsCalculator>();

            sequencerDimensionsCalculatorMock
                .Setup(x => x.FindNoteFromPoint(sequencerNotesMock.Object, mousePoint))
                .Returns(visualNoteMock.Object);

            var noteStateCommandFactoryMock = new Mock<INoteStateCommandFactory>();

            var nselectCommandMock = new Mock<INotesCommand>();
            noteStateCommandFactoryMock.Setup(x => x.CreateNoteStateCommand(NoteState.Selected)).Returns(nselectCommandMock.Object);

            var command = new UpdateNoteStateFromPointCommand(sequencerNotesMock.Object, mouseOperatorMock.Object,
                keyboardStateProcessorMock.Object, sequencerDimensionsCalculatorMock.Object, noteStateCommandFactoryMock.Object);

            command.Execute(mousePoint);

            nselectCommandMock.Verify(x => x.Execute(new List<IVisualNote> { visualNoteMock.Object }));
        }
    }
}