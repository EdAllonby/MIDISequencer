using System.Collections.Generic;
using System.Windows.Input;
using Moq;
using NUnit.Framework;
using Sequencer.View.Command.NotesCommand;
using Sequencer.Visual;
using Sequencer.Visual.Input;

namespace Sequencer.View.Tests.CommandTests.NotesCommandTests
{
    [TestFixture]
    public class UpdateNoteStateCommandTests
    {
        [Test]
        public void UpdateNoteState_WithCtrlPressed()
        {
            const NoteState expectedNoteState = NoteState.Selected;

            var sequencerNotesMock = new Mock<ISequencerNotes>();
            var keyboardStateMock = new Mock<IKeyboardStateProcessor>();
            keyboardStateMock.Setup(x => x.IsKeyDown(Key.LeftCtrl)).Returns(true);

            var command = new UpdateNoteStateCommand(sequencerNotesMock.Object, keyboardStateMock.Object, expectedNoteState);

            var note1 = new Mock<IVisualNote>();
            note1.Setup(x => x.NoteState).Returns(NoteState.Unselected);

            var note2 = new Mock<IVisualNote>();
            note2.Setup(x => x.NoteState).Returns(NoteState.Selected);

            command.Execute(new List<IVisualNote> { note1.Object, note2.Object });

            note1.VerifySet(x => x.NoteState = expectedNoteState, Times.Once);
            note2.VerifySet(x => x.NoteState = expectedNoteState, Times.Never);
        }

        [Test]
        public void UpdateNoteState_WithoutCtrlPressed_ButUnselectedNotes()
        {
            const NoteState expectedNoteState = NoteState.Selected;

            var sequencerNotesMock = new Mock<ISequencerNotes>();

            var keyboardStateMock = new Mock<IKeyboardStateProcessor>();
            keyboardStateMock.Setup(x => x.IsKeyDown(Key.LeftCtrl)).Returns(false);

            var command = new UpdateNoteStateCommand(sequencerNotesMock.Object, keyboardStateMock.Object, expectedNoteState);

            var note1 = new Mock<IVisualNote>();
            note1.Setup(x => x.NoteState).Returns(NoteState.Unselected);

            var note2 = new Mock<IVisualNote>();
            note2.Setup(x => x.NoteState).Returns(NoteState.Selected);

            var notesToChange = new List<IVisualNote> { note1.Object, note2.Object };

            sequencerNotesMock.Setup(x => x.FindAllOtherNotes(notesToChange)).Returns(notesToChange);

            command.Execute(notesToChange);

            note1.VerifySet(x => x.NoteState = expectedNoteState, Times.Once);
            note2.VerifySet(x => x.NoteState = NoteState.Unselected, Times.Once);
        }
    }
}