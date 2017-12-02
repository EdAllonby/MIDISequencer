﻿using Moq;
using NUnit.Framework;
using Sequencer.Domain;
using Sequencer.Shared;
using Sequencer.View.Command.MousePointCommand;
using Sequencer.View.Drawing;
using Sequencer.View.Input;
using Sequencer.View.Control;

namespace Sequencer.View.Tests.CommandTests.MousePointCommandTests
{
    [TestFixture]
    public sealed class CreateNoteFromPointCommandTests
    {
        [Test]
        public void CreateNoteTest()
        {
            var mockVisualNoteFactory = new Mock<IVisualNoteFactory>();
            var mockSequencerNotes = new Mock<ISequencerNotes>();
            var mockSequencerCalculator = new Mock<ISequencerDimensionsCalculator>();
            var mockMousePoint = new Mock<IMousePoint>();
            var mockNoteToDelete = new Mock<IVisualNote>();
            var mockMouseOperator = new Mock<IMouseOperator>();
            var mockNotePosition = new Mock<IPosition>();
            var mockNextPosition = new Mock<IPosition>();
            var mockCreatedNote = new Mock<IVisualNote>();

            mockNotePosition.Setup(x => x.NextPosition(It.IsAny<TimeSignature>())).Returns(mockNextPosition.Object);
            mockMouseOperator.Setup(x => x.CanModifyNote).Returns(true);
            mockSequencerCalculator.Setup(x => x.FindNoteFromPoint(mockSequencerNotes.Object, mockMousePoint.Object)).Returns(mockNoteToDelete.Object);
            mockSequencerCalculator.Setup(x => x.FindPositionFromPoint(mockMousePoint.Object)).Returns(mockNotePosition.Object);
            mockVisualNoteFactory.Setup(x => x.CreateNote(It.IsAny<Pitch>(), mockNotePosition.Object, mockNextPosition.Object)).Returns(mockCreatedNote.Object);

            var command = new CreateNoteFromPointCommand(mockVisualNoteFactory.Object, mockSequencerNotes.Object, new SequencerSettings(), mockMouseOperator.Object, mockSequencerCalculator.Object);
            command.Execute(mockMousePoint.Object);

            mockSequencerNotes.Verify(x=>x.MakeAllUnselected(), Times.Once);
            mockSequencerNotes.Verify(x=>x.AddNote(mockCreatedNote.Object), Times.Once);
        }
    }
}