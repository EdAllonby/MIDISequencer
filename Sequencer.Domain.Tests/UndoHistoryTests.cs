using NUnit.Framework;
using Sequencer.Domain.Arrangement;

namespace Sequencer.Domain.Tests
{
    [TestFixture]
    public class UndoHistoryTests
    {
        [Test]
        public void UndoAndRedo_StepThroughTheRecordedSongs()
        {
            var history = new UndoHistory();
            history.Reset("a");
            history.Record("b");
            history.Record("c");

            Assert.That(history.Undo(), Is.EqualTo("b"));
            Assert.That(history.Undo(), Is.EqualTo("a"));
            Assert.That(history.Undo(), Is.Null);
            Assert.That(history.Redo(), Is.EqualTo("b"));
            Assert.That(history.Redo(), Is.EqualTo("c"));
            Assert.That(history.Redo(), Is.Null);
        }

        [Test]
        public void ANewEdit_AfterUndoing_DiscardsTheRedo()
        {
            var history = new UndoHistory();
            history.Reset("a");
            history.Record("b");
            history.Undo();

            history.Record("x");

            Assert.That(history.CanRedo, Is.False);
            Assert.That(history.Undo(), Is.EqualTo("a"));
        }

        [Test]
        public void RecordingTheSameSong_IsNotAnEdit()
        {
            var history = new UndoHistory();
            history.Reset("a");

            Assert.That(history.Record("a"), Is.False);
            Assert.That(history.CanUndo, Is.False);
        }

        [Test]
        public void Reset_ForgetsTheHistory()
        {
            var history = new UndoHistory();
            history.Reset("a");
            history.Record("b");

            history.Reset("new");

            Assert.That(history.CanUndo, Is.False);
            Assert.That(history.CanRedo, Is.False);
        }

        [Test]
        public void TheHistory_IsLimited()
        {
            var history = new UndoHistory();
            history.Reset("0");

            for (var i = 1; i <= UndoHistory.Limit + 50; i++)
            {
                history.Record(i.ToString());
            }

            var steps = 0;

            while (history.Undo() != null)
            {
                steps++;
            }

            Assert.That(steps, Is.EqualTo(UndoHistory.Limit));
        }
    }
}
