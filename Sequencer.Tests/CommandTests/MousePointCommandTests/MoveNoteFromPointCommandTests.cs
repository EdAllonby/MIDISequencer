using System.Collections.Generic;
using System.Windows.Input;
using Moq;
using NUnit.Framework;
using Sequencer.Command.MousePointCommand;
using Sequencer.Domain;
using Sequencer.Drawing;
using Sequencer.Input;
using Sequencer.View;

namespace Sequencer.Tests.CommandTests.MousePointCommandTests
{
    [TestFixture]
    public sealed class MoveNoteFromPointCommandTests
    {
        [Test]
        public void MoveNotePosition_WithNoMovement_DoesNothing()
        {
            var mockKeyboardStateProcessor = new Mock<IKeyboardStateProcessor>();
            var mockSequencerNotes = new Mock<ISequencerNotes>();
            var mockSequencerCalculator = new Mock<ISequencerDimensionsCalculator>();
            var mockInitialMousePoint = new Mock<IMousePoint>();
            var mockPitchStepCalculator = new Mock<IPitchAndPositionCalculator>();
            var mockMousePoint = new Mock<IMousePoint>();
            var mockNoteToMove = new Mock<IVisualNote>();
            var mockMouseOperator = new Mock<IMouseOperator>();
            var startPosition = new Mock<IPosition>();
            var endPosition = new Mock<IPosition>();
            var mockNextPosition = new Mock<IPosition>();

            mockPitchStepCalculator.Setup(x => x.FindStepsFromPitches(It.IsAny<Pitch>(), It.IsAny<Pitch>())).Returns(0);
            endPosition.Setup(x => x.NextPosition(It.IsAny<TimeSignature>())).Returns(mockNextPosition.Object);
            mockMouseOperator.Setup(x => x.CanModifyNote).Returns(true);

            mockSequencerNotes.Setup(x => x.SelectedNotes).Returns(new List<IVisualNote> { mockNoteToMove.Object });
            mockSequencerCalculator.Setup(x => x.FindPositionFromPoint(mockInitialMousePoint.Object)).Returns(startPosition.Object);
            mockSequencerCalculator.Setup(x => x.FindPositionFromPoint(mockMousePoint.Object)).Returns(endPosition.Object);

            var command = new MoveNoteFromPointCommand(mockKeyboardStateProcessor.Object, mockPitchStepCalculator.Object, mockInitialMousePoint.Object, mockMouseOperator.Object, mockSequencerNotes.Object, mockSequencerCalculator.Object);
            command.Execute(mockMousePoint.Object);

            mockNoteToMove.Verify(x => x.MovePositionRelativeTo(It.IsAny<int>()), Times.Never);
        }

        [Test]
        public void MoveNotePosition_WithSmallMovement_MovesNotePositionByThatAmount()
        {
            var mockKeyboardStateProcessor = new Mock<IKeyboardStateProcessor>();
            var mockSequencerNotes = new Mock<ISequencerNotes>();
            var mockSequencerCalculator = new Mock<ISequencerDimensionsCalculator>();
            var mockInitialMousePoint = new Mock<IMousePoint>();
            var calculator = new Mock<IPitchAndPositionCalculator>();
            var mockMousePoint = new Mock<IMousePoint>();
            var mockNoteToMove = new Mock<IVisualNote>();
            var mockMouseOperator = new Mock<IMouseOperator>();
            var startPosition = new Mock<IPosition>();
            var endPosition = new Mock<IPosition>();
            var mockNextPosition = new Mock<IPosition>();

            var relativeBeatsToMove = 12;

            mockKeyboardStateProcessor.Setup(x => x.IsKeyDown(Key.LeftShift)).Returns(false);
            calculator.Setup(x => x.FindBeatsBetweenPositions(It.IsAny<IPosition>(), It.IsAny<IPosition>())).Returns(relativeBeatsToMove);
            endPosition.Setup(x => x.NextPosition(It.IsAny<TimeSignature>())).Returns(mockNextPosition.Object);
            mockMouseOperator.Setup(x => x.CanModifyNote).Returns(true);

            mockSequencerNotes.Setup(x => x.SelectedNotes).Returns(new List<IVisualNote> { mockNoteToMove.Object });

            mockSequencerCalculator.Setup(x => x.FindPositionFromPoint(mockInitialMousePoint.Object)).Returns(startPosition.Object);
            mockSequencerCalculator.Setup(x => x.FindPositionFromPoint(mockMousePoint.Object)).Returns(endPosition.Object);

            var command = new MoveNoteFromPointCommand(mockKeyboardStateProcessor.Object, calculator.Object, mockInitialMousePoint.Object, mockMouseOperator.Object, mockSequencerNotes.Object, mockSequencerCalculator.Object);
            command.Execute(mockMousePoint.Object);

            mockNoteToMove.Verify(x => x.MovePositionRelativeTo(relativeBeatsToMove), Times.Once);
        }

        [Test]
        public void MoveNotePitch_WithSmallMovement_MovesNotePitchByThatAmount()
        {
            var mockKeyboardStateProcessor = new Mock<IKeyboardStateProcessor>();
            var mockSequencerNotes = new Mock<ISequencerNotes>();
            var mockSequencerCalculator = new Mock<ISequencerDimensionsCalculator>();
            var mockInitialMousePoint = new Mock<IMousePoint>();
            var calculator = new Mock<IPitchAndPositionCalculator>();
            var mockMousePoint = new Mock<IMousePoint>();
            var mockNoteToMove = new Mock<IVisualNote>();
            var mockMouseOperator = new Mock<IMouseOperator>();
            var startPosition = new Mock<IPosition>();
            var endPosition = new Mock<IPosition>();
            var mockNextPosition = new Mock<IPosition>();

            var relativePitchesToMove = 2;

            mockKeyboardStateProcessor.Setup(x => x.IsKeyDown(Key.LeftShift)).Returns(false);
            calculator.Setup(x => x.FindStepsFromPitches(It.IsAny<Pitch>(), It.IsAny<Pitch>())).Returns(relativePitchesToMove);
            endPosition.Setup(x => x.NextPosition(It.IsAny<TimeSignature>())).Returns(mockNextPosition.Object);
            mockMouseOperator.Setup(x => x.CanModifyNote).Returns(true);

            mockSequencerNotes.Setup(x => x.SelectedNotes).Returns(new List<IVisualNote> { mockNoteToMove.Object });

            mockSequencerCalculator.Setup(x => x.FindPositionFromPoint(mockInitialMousePoint.Object)).Returns(startPosition.Object);
            mockSequencerCalculator.Setup(x => x.FindPositionFromPoint(mockMousePoint.Object)).Returns(endPosition.Object);

            var command = new MoveNoteFromPointCommand(mockKeyboardStateProcessor.Object, calculator.Object, mockInitialMousePoint.Object, mockMouseOperator.Object, mockSequencerNotes.Object, mockSequencerCalculator.Object);
            command.Execute(mockMousePoint.Object);

            mockNoteToMove.Verify(x => x.MovePitchRelativeTo(relativePitchesToMove), Times.Once);
        }
    }
}