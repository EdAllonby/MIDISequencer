using System.Collections.Generic;
using Moq;
using NUnit.Framework;
using Sequencer.Command.NotesCommand;
using Sequencer.View;

namespace Sequencer.Tests.CommandTests.NotesCommandTests
{
    [TestFixture]
    public class MoveNotePitchCommandTests
    {
        [Test]
        public void MoveNotePitchTests()
        {
            const int halfStepMove = 2;

            var command = new MoveNotePitchCommand(halfStepMove);

            var deleteNote1 = new Mock<IVisualNote>();
            var deleteNote2 = new Mock<IVisualNote>();
            var deleteNote3 = new Mock<IVisualNote>();

            command.Execute(new List<IVisualNote> { deleteNote1.Object, deleteNote2.Object, deleteNote3.Object });

            deleteNote1.Verify(x=>x.MovePitchRelativeTo(halfStepMove));
            deleteNote2.Verify(x => x.MovePitchRelativeTo(halfStepMove));
            deleteNote3.Verify(x => x.MovePitchRelativeTo(halfStepMove));

        }
    }
}