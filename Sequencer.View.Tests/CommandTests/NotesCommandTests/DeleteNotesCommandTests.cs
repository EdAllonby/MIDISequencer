using System.Collections.Generic;
using Moq;
using NUnit.Framework;
using Sequencer.View.Command.NotesCommand;
using Sequencer.View.Control;

namespace Sequencer.View.Tests.CommandTests.NotesCommandTests
{
    [TestFixture]
    public class DeleteNotesCommandTests
    {
        [Test]
        public void DeleteNoteTest()
        {
            var mockSequencerNotes = new Mock<ISequencerNotes>();

            var command = new DeleteNotesCommand(mockSequencerNotes.Object);

            var deleteNote1 = new Mock<IVisualNote>();
            var deleteNote2 = new Mock<IVisualNote>();
            var deleteNote3 = new Mock<IVisualNote>();

            command.Execute(new List<IVisualNote> { deleteNote1.Object, deleteNote2.Object, deleteNote3.Object });

            mockSequencerNotes.Verify(x => x.DeleteNote(deleteNote1.Object));
            mockSequencerNotes.Verify(x => x.DeleteNote(deleteNote2.Object));
            mockSequencerNotes.Verify(x => x.DeleteNote(deleteNote3.Object));
        }
    }
}