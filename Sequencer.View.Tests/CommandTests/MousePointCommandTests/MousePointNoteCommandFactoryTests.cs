using Moq;
using NUnit.Framework;
using Sequencer.Domain.Settings;
using Sequencer.View.Command.MousePointCommand;
using Sequencer.ViewModel;
using Sequencer.Visual;
using Sequencer.Visual.Input;

namespace Sequencer.View.Tests.CommandTests.MousePointCommandTests
{
    [TestFixture]
    public sealed class MousePointNoteCommandFactoryTests
    {
        [SetUp]
        public void BeforeEachTest()
        {
            var mockVisualNoteFactory = new Mock<IVisualNoteFactory>();
            var mockSequencerNotes = new Mock<ISequencerNotes>();
            var keyboardStateStub = new Mock<IKeyboardStateProcessor>();
            var mockSequencerCalculator = new Mock<ISequencerDimensionsCalculator>();
            var mockMouseOperator = new Mock<IMouseOperator>();
            var mockNoteStateCommandFactory = new Mock<INoteStateCommandFactory>();
            factory = new MousePointNoteCommandFactory(mockVisualNoteFactory.Object, mockMouseOperator.Object, keyboardStateStub.Object, mockSequencerNotes.Object, new SequencerSettings(), mockSequencerCalculator.Object, mockNoteStateCommandFactory.Object);
        }

        private MousePointNoteCommandFactory factory;

        [Test]
        public void CreateReturnsCreateCommand()
        {
            IMousePointNoteCommand mouseCommand = factory.FindCommand(NoteAction.Create);

            Assert.IsInstanceOf<CreateNoteFromPointCommand>(mouseCommand);
        }

        [Test]
        public void DeleteReturnsCreateCommand()
        {
            IMousePointNoteCommand mouseCommand = factory.FindCommand(NoteAction.Delete);

            Assert.IsInstanceOf<DeleteNoteFromPointCommand>(mouseCommand);
        }


        [Test]
        public void FindCommand_WithUnsupportedNoteAction_ReturnsEmptyMousePointCommand()
        {
            IMousePointNoteCommand mouseCommand = factory.FindCommand(NoteAction.None);

            Assert.IsInstanceOf<EmptyMousePointCommand>(mouseCommand);
        }

        [Test]
        public void SelectReturnsCreateCommand()
        {
            IMousePointNoteCommand mouseCommand = factory.FindCommand(NoteAction.Select);

            Assert.IsInstanceOf<UpdateNoteStateFromPointCommand>(mouseCommand);
        }
    }
}