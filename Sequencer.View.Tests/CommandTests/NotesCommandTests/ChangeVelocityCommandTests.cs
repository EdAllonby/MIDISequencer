using System.Collections.Generic;
using Moq;
using NUnit.Framework;
using Sequencer.Domain;
using Sequencer.View.Command.NotesCommand;
using Sequencer.View.Control;

namespace Sequencer.View.Tests.CommandTests.NotesCommandTests
{
    [TestFixture]
    public class ChangeVelocityCommandTests
    {
        [Test]
        public void ChangeNoteVelocity()
        {
            var velocity = new Velocity(10);
            var command = new ChangeVelocityCommand(velocity);

            var note = new Mock<IVisualNote>();

            command.Execute(new List<IVisualNote> { note.Object });

            note.VerifySet(x => x.Velocity = velocity, Times.Once);
        }
    }
}