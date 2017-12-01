using System.Collections.Generic;
using System.Linq;
using Moq;
using NUnit.Framework;
using Sequencer.Command.MousePointCommand;
using Sequencer.Command.NotesCommand;
using Sequencer.Drawing;
using Sequencer.View;

namespace Sequencer.Tests.CommandTests.MousePointCommandTests
{
    [TestFixture]
    public sealed class DeleteNoteFromPointCommandTests
    {
        [Test]
        public void DeleteNoteTest()
        {
            var mockSequencerNotes = new Mock<ISequencerNotes>();
            var sequencerCalculatorStub = new Mock<ISequencerDimensionsCalculator>();
            var mockMousePoint = new Mock<IMousePoint>();
            var mockNoteToDelete = new Mock<IVisualNote>();
            var mockDeleteCommand = new Mock<IDeleteNotesCommand>();

            sequencerCalculatorStub.Setup(x => x.FindNoteFromPoint(mockSequencerNotes.Object, mockMousePoint.Object)).Returns(mockNoteToDelete.Object);

            var command = new DeleteNoteFromPointCommand(mockSequencerNotes.Object, sequencerCalculatorStub.Object, mockDeleteCommand.Object);
            command.Execute(mockMousePoint.Object);

            mockDeleteCommand.Verify(x => x.Execute(It.Is<IEnumerable<IVisualNote>>(m => m.Contains(mockNoteToDelete.Object) && m.Count() == 1)), Times.Once);
        }
    }
}