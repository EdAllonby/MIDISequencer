using System.Collections.Generic;
using Moq;
using NUnit.Framework;
using Sequencer.View.Command.NotesCommand;
using Sequencer.View.Control;

namespace Sequencer.View.Tests.CommandTests.NotesCommandTests
{
    [TestFixture]
    public class MoveNotePositionCommandTests
    {
        [Test]
        public void MoveNotePositionTests()
        {
            const int beatsToMove = 7;

            var command = new MoveNotePositionCommand(beatsToMove);

            var deleteNote1 = new Mock<IVisualNote>();
            var deleteNote2 = new Mock<IVisualNote>();
            var deleteNote3 = new Mock<IVisualNote>();

            command.Execute(new List<IVisualNote> { deleteNote1.Object, deleteNote2.Object, deleteNote3.Object });

            deleteNote1.Verify(x => x.MovePositionRelativeTo(beatsToMove));
            deleteNote2.Verify(x => x.MovePositionRelativeTo(beatsToMove));
            deleteNote3.Verify(x => x.MovePositionRelativeTo(beatsToMove));
        }
    }
}