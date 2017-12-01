using Moq;
using NUnit.Framework;
using Sequencer.Command.MousePointCommand;
using Sequencer.Domain;
using Sequencer.Drawing;
using Sequencer.Input;
using Sequencer.View;
using Sequencer.ViewModel;

namespace Sequencer.Tests.CommandTests.MousePointCommandTests
{
    [TestFixture]
    public sealed class MousePointNoteCommandFactoryTests
    {
        private MousePointNoteCommandFactory factory;

        [SetUp]
        public void BeforeEachTest()
        {
            var mockVisualNoteFactory = new Mock<IVisualNoteFactory>();
            var mockSequencerNotes = new Mock<ISequencerNotes>();
            var keyboardStateStub = new Mock<IKeyboardStateProcessor>();
            var mockSequencerCalculator = new Mock<ISequencerDimensionsCalculator>();
            var mockMouseOperator = new Mock<IMouseOperator>();

            factory = new MousePointNoteCommandFactory(mockVisualNoteFactory.Object, mockMouseOperator.Object, keyboardStateStub.Object, mockSequencerNotes.Object, new SequencerSettings(), mockSequencerCalculator.Object);
        }

        [Test]
        public void CreateReturnsCreateCommand()
        {
            IMousePointNoteCommand mouseCommand = factory.FindCommand(NoteAction.Create);

            Assert.IsInstanceOf<CreateNoteFromPointCommand>(mouseCommand);
        }

        [Test]
        public void SelectReturnsCreateCommand()
        {
            IMousePointNoteCommand mouseCommand = factory.FindCommand(NoteAction.Select);

            Assert.IsInstanceOf<UpdateNoteStateFromPointCommand>(mouseCommand);
        }

        [Test]
        public void DeleteReturnsCreateCommand()
        {
            IMousePointNoteCommand mouseCommand = factory.FindCommand(NoteAction.Delete);

            Assert.IsInstanceOf<DeleteNoteFromPointCommand>(mouseCommand);
        }
    }
}