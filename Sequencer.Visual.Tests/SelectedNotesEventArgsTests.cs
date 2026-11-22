using System.Collections.Generic;
using Moq;
using NUnit.Framework;
using NUnit.Framework.Legacy;

namespace Sequencer.Visual.Tests
{
    [TestFixture]
    public class SelectedNotesEventArgsTests
    {
        [Test]
        public void SelectedNotesEventArgs_ShouldContainSelectedNotes()
        {
            var expectedNotes = new List<IVisualNote> { It.IsAny<IVisualNote>() };

            var args = new SelectedNotesEventArgs(expectedNotes);

            CollectionAssert.AreEqual(expectedNotes, args.SelectedVisualNotes);
        }
    }
}