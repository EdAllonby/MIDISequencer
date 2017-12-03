using NUnit.Framework;

namespace Sequencer.Domain.Tests
{
    [TestFixture]
    public class NoteTests
    {
        [Test]
        public void AccidentalNoteShouldBeAccidental()
        {
            bool isPitchAccidental = Note.ASharp.IsAccidental;

            Assert.IsTrue(isPitchAccidental);
        }

        [Test]
        public void FindFromDisplayNameGetsCorrectType()
        {
            const string displayName = "A";

            Note noteA = Note.FromDisplayName(displayName);

            Assert.AreEqual(noteA.DisplayName, displayName);
        }

        [Test]
        public void NextElementGetsNextElementInEnumeration()
        {
            Note element = Note.A;

            Note nextPitch = Note.GetNextElement(element);

            Note expectedNote = Note.ASharp;

            Assert.AreEqual(expectedNote, nextPitch);
        }
    }
}