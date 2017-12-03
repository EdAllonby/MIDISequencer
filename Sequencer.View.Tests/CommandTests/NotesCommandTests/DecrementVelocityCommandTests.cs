using System.Collections.Generic;
using Moq;
using NUnit.Framework;
using Sequencer.Domain;
using Sequencer.View.Command.NotesCommand;
using Sequencer.View.Control;

namespace Sequencer.View.Tests.CommandTests.NotesCommandTests
{
    [TestFixture]
    public class DecrementVelocityCommandTests
    {
        [Test]
        public void DecrementVelocity_NearMinimum_SetsAsOne()
        {
            var velocity = new Velocity(5);
            var expectedVelocity = new Velocity(0);

            var command = new DecrementVelocityCommand(10);

            var note = new Mock<IVisualNote>();
            note.SetupGet(x => x.Velocity).Returns(velocity);

            command.Execute(new List<IVisualNote> { note.Object });

            note.VerifySet(x => x.Velocity = expectedVelocity, Times.Once);
        }

        [Test]
        public void DecrementVelocityTest()
        {
            var velocity = new Velocity(100);
            var expectedVelocity = new Velocity(90);

            var command = new DecrementVelocityCommand(10);

            var note = new Mock<IVisualNote>();
            note.SetupGet(x => x.Velocity).Returns(velocity);

            command.Execute(new List<IVisualNote> { note.Object });

            note.VerifySet(x => x.Velocity = expectedVelocity, Times.Once);
        }
    }
}