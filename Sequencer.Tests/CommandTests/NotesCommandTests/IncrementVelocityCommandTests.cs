using System.Collections.Generic;
using Moq;
using NUnit.Framework;
using Sequencer.Command.NotesCommand;
using Sequencer.Domain;
using Sequencer.View;

namespace Sequencer.Tests.CommandTests.NotesCommandTests
{
    [TestFixture]
    public class IncrementVelocityCommandTests
    {
        [Test]
        public void IncrementVelocityTest()
        {
            var velocity = new Velocity(100);
            var expectedVelocity = new Velocity(110);

            var command = new IncrementVelocityCommand(10);

            var note = new Mock<IVisualNote>();
            note.SetupGet(x => x.Velocity).Returns(velocity);

            command.Execute(new List<IVisualNote> { note.Object });

            note.VerifySet(x => x.Velocity = expectedVelocity, Times.Once);
        }

        [Test]
        public void IncrementVelocity_NearMaximum_SetsAs127()
        {
            var velocity = new Velocity(124);
            var expectedVelocity = new Velocity(127);

            var command = new IncrementVelocityCommand(10);

            var note = new Mock<IVisualNote>();
            note.SetupGet(x => x.Velocity).Returns(velocity);

            command.Execute(new List<IVisualNote> { note.Object });

            note.VerifySet(x => x.Velocity = expectedVelocity, Times.Once);
        }
    }
}