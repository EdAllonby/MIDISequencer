using Moq;
using NUnit.Framework;
using Sequencer.Shared;
using Sequencer.View.Command.MousePointCommand;
using Sequencer.View.Control;
using Sequencer.View.Drawing;
using Sequencer.View.Input;
using Sequencer.ViewModel;

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

            factory = new MousePointNoteCommandFactory(mockVisualNoteFactory.Object, mockMouseOperator.Object, keyboardStateStub.Object, mockSequencerNotes.Object, new SequencerSettings(), mockSequencerCalculator.Object);
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
        public void SelectReturnsCreateCommand()
        {
            IMousePointNoteCommand mouseCommand = factory.FindCommand(NoteAction.Select);

            Assert.IsInstanceOf<UpdateNoteStateFromPointCommand>(mouseCommand);
        }
    }
}