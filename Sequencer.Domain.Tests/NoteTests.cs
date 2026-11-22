using NUnit.Framework;
using NUnit.Framework.Legacy;

namespace Sequencer.Domain.Tests
{
    [TestFixture]
    public class NoteTests
    {
        [Test]
        public void AccidentalNoteShouldBeAccidental()
        {
            bool isPitchAccidental = Note.ASharp.IsAccidental;

            ClassicAssert.IsTrue(isPitchAccidental);
        }

        [Test]
        public void FindFromDisplayNameGetsCorrectType()
        {
            const string displayName = "A";

            Note noteA = Note.FromDisplayName(displayName);

            ClassicAssert.AreEqual(noteA.DisplayName, displayName);
        }

        [Test]
        public void NextElementGetsNextElementInEnumeration()
        {
            Note element = Note.A;

            Note nextPitch = Note.GetNextElement(element);

            Note expectedNote = Note.ASharp;

            ClassicAssert.AreEqual(expectedNote, nextPitch);
        }
    }
}