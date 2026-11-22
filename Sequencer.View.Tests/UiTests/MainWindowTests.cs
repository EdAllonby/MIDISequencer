using System;
using System.IO;
using System.Linq;
using Autofac;
using Avalonia;
using Avalonia.Headless;
using Avalonia.Headless.NUnit;
using Avalonia.Input;
using Avalonia.Threading;
using NUnit.Framework;
using Sequencer.View.Control;
using Sequencer.ViewModel;
using Sequencer.Visual;
using Sequencer.Visual.Input;

namespace Sequencer.View.Tests.UiTests
{
    /// <summary>
    /// Drives the real main window with simulated input. The window is 1200 x 500: the editor fills the
    /// left 895 px below a 30 px header, so one beat is about 224 px wide and one note lane 12.5 px tall.
    /// </summary>
    [TestFixture]
    public class MainWindowTests
    {
        private IContainer container;
        private MainWindow window;
        private ISequencerNotes notes;
        private SequencerViewModel viewModel;
        private RawInputModifiers commandModifier;

        private static readonly Point EditorPoint = new Point(120, 230);

        [SetUp]
        public void SetUp()
        {
            container = App.BuildContainer();
            window = container.Resolve<MainWindow>();
            notes = container.Resolve<ISequencerNotes>();
            viewModel = container.Resolve<SequencerViewModel>();
            commandModifier = container.Resolve<KeyboardStateProcessor>().CommandModifier == KeyModifiers.Meta
                ? RawInputModifiers.Meta
                : RawInputModifiers.Control;

            window.Show();
            Dispatcher.UIThread.RunJobs();
        }

        [TearDown]
        public void TearDown()
        {
            window.Close();
            container.Dispose();
        }

        private void Click(Point point, MouseButton button = MouseButton.Left, RawInputModifiers modifiers = RawInputModifiers.None)
        {
            window.MouseDown(point, button, modifiers);
            window.MouseUp(point, button, modifiers);
            Dispatcher.UIThread.RunJobs();
        }

        private void Press(Key key, RawInputModifiers modifiers = RawInputModifiers.None)
        {
            window.KeyPress(key, modifiers, PhysicalKey.None, null);
            window.KeyRelease(key, modifiers, PhysicalKey.None, null);
            Dispatcher.UIThread.RunJobs();
        }

        [AvaloniaTest]
        public void Window_RendersTheEditor()
        {
            string path = Path.Combine(TestContext.CurrentContext.WorkDirectory, "main-window.png");
            window.CaptureRenderedFrame()?.Save(path);
            TestContext.Out.WriteLine($"Rendered frame: {path}");

            Assert.That(File.Exists(path), Is.True);
            Assert.That(viewModel.NoteAction, Is.EqualTo(NoteAction.Create));
        }

        [AvaloniaTest]
        public void Window_RendersNotesAndTheRadialMenu()
        {
            Click(EditorPoint);
            Click(EditorPoint + new Point(170, -40));
            Click(EditorPoint + new Point(340, -80));
            window.MouseDown(new Point(700, 300), MouseButton.Right);
            window.MouseMove(new Point(730, 300));

            string path = Path.Combine(TestContext.CurrentContext.WorkDirectory, "notes-and-menu.png");
            window.CaptureRenderedFrame()?.Save(path);
            TestContext.Out.WriteLine($"Rendered frame: {path}");
            window.MouseUp(new Point(730, 300), MouseButton.Right);

            Assert.That(notes.AllNotes.Count(), Is.EqualTo(3));
        }

        [AvaloniaTest]
        public void Click_InCreateMode_CreatesASelectedNote()
        {
            Click(EditorPoint);

            Assert.That(notes.AllNotes.Count(), Is.EqualTo(1));
            Assert.That(notes.AllNotes.Single().NoteState, Is.EqualTo(NoteState.Selected));
        }

        [AvaloniaTest]
        public void Drag_InCreateMode_LengthensTheNewNote()
        {
            window.MouseDown(EditorPoint, MouseButton.Left);
            window.MouseMove(EditorPoint + new Point(300, 0));
            window.MouseUp(EditorPoint + new Point(300, 0), MouseButton.Left);

            IVisualNote note = notes.AllNotes.Single();
            int lengthInTicks = note.EndPosition.TotalTicks(Domain.TimeSignature.FourFour, 96) -
                                note.StartPosition.TotalTicks(Domain.TimeSignature.FourFour, 96);
            Assert.That(lengthInTicks, Is.GreaterThan(24), "Longer than the default sixteenth note.");
        }

        [AvaloniaTest]
        public void Backspace_DeletesSelectedNotes()
        {
            // A Mac keyboard's "delete" key sends Backspace.
            Click(EditorPoint);

            Press(Key.Back);

            Assert.That(notes.AllNotes, Is.Empty);
        }

        [AvaloniaTest]
        public void CommandA_SelectsAllNotes_AndEscapeDeselects()
        {
            Click(EditorPoint);
            Click(EditorPoint + new Point(300, 50));
            Assert.That(notes.SelectedNotes.Count(), Is.EqualTo(1), "Creating a note deselects the others.");

            Press(Key.A, commandModifier);
            Assert.That(notes.SelectedNotes.Count(), Is.EqualTo(2));

            Press(Key.Escape);
            Assert.That(notes.SelectedNotes, Is.Empty);
        }

        [AvaloniaTest]
        public void UpArrow_RaisesTheSelectedNoteBySemitone()
        {
            Click(EditorPoint);
            string before = notes.AllNotes.Single().Pitch.ToString();

            Press(Key.Up);

            Assert.That(notes.AllNotes.Single().Pitch.ToString(), Is.Not.EqualTo(before));
        }

        [AvaloniaTest]
        public void EqualsKey_IncreasesVelocity()
        {
            Click(EditorPoint);
            int before = notes.AllNotes.Single().Velocity.Value;

            Press(Key.OemPlus);

            Assert.That(notes.AllNotes.Single().Velocity.Value, Is.EqualTo(before + 5));
        }

        [AvaloniaTest]
        public void RightDrag_ToTheRight_SelectsDeleteInTheRadialMenu()
        {
            window.MouseDown(EditorPoint, MouseButton.Right);
            window.MouseMove(EditorPoint + new Point(30, 0));
            window.MouseUp(EditorPoint + new Point(30, 0), MouseButton.Right);
            Dispatcher.UIThread.RunJobs();

            Assert.That(viewModel.NoteAction, Is.EqualTo(NoteAction.Delete));
            Assert.That(notes.AllNotes, Is.Empty, "A right-click must not create a note.");
        }

        [AvaloniaTest]
        public void RightDrag_Upwards_SelectsCreate_AndDownLeftSelectsSelect()
        {
            window.MouseDown(EditorPoint, MouseButton.Right);
            window.MouseMove(EditorPoint + new Point(-20, 20));
            window.MouseUp(EditorPoint + new Point(-20, 20), MouseButton.Right);
            Assert.That(viewModel.NoteAction, Is.EqualTo(NoteAction.Select));

            window.MouseDown(EditorPoint, MouseButton.Right);
            window.MouseMove(EditorPoint + new Point(0, -30));
            window.MouseUp(EditorPoint + new Point(0, -30), MouseButton.Right);
            Assert.That(viewModel.NoteAction, Is.EqualTo(NoteAction.Create));
        }

        [AvaloniaTest]
        public void ControlClick_OnMac_OpensTheRadialMenuInsteadOfCreatingANote()
        {
            if (!OperatingSystem.IsMacOS())
            {
                Assert.Ignore("Ctrl-click is a secondary click only on macOS.");
            }

            window.MouseDown(EditorPoint, MouseButton.Left, RawInputModifiers.Control);
            window.MouseMove(EditorPoint + new Point(30, 0), RawInputModifiers.Control);
            window.MouseUp(EditorPoint + new Point(30, 0), MouseButton.Left, RawInputModifiers.Control);

            Assert.That(viewModel.NoteAction, Is.EqualTo(NoteAction.Delete));
            Assert.That(notes.AllNotes, Is.Empty);
        }

        [AvaloniaTest]
        public void SelectMode_DragBox_SelectsTheNotesInside()
        {
            Click(EditorPoint);
            Click(EditorPoint + new Point(300, 0));
            Press(Key.Escape);
            viewModel.NoteAction = NoteAction.Select;

            // Drag a box around the first note only.
            window.MouseDown(EditorPoint + new Point(-40, -20), MouseButton.Left);
            window.MouseMove(EditorPoint + new Point(60, 20));
            window.MouseUp(EditorPoint + new Point(60, 20), MouseButton.Left);

            Assert.That(notes.SelectedNotes.Count(), Is.EqualTo(1));
        }

        [AvaloniaTest]
        public void SelectMode_CommandClick_AddsToTheSelection()
        {
            Click(EditorPoint);
            Click(EditorPoint + new Point(300, 0));
            Press(Key.Escape);
            viewModel.NoteAction = NoteAction.Select;

            Click(EditorPoint);
            Click(EditorPoint + new Point(300, 0), modifiers: commandModifier);

            Assert.That(notes.SelectedNotes.Count(), Is.EqualTo(2));
        }

        [AvaloniaTest]
        public void SelectingOneNote_ShowsItInTheInspector()
        {
            Click(EditorPoint);

            Assert.That(viewModel.SelectedNote, Is.Not.Null);
            Assert.That(viewModel.SelectedNote.Velocity, Is.EqualTo(64));
        }

        [AvaloniaTest]
        public void Space_TogglesPlayAndPause_AndReturnStops()
        {
            Press(Key.Space);
            Assert.That(viewModel.SequencerPlayState, Is.EqualTo(PlayState.Play));

            Press(Key.Space);
            Assert.That(viewModel.SequencerPlayState, Is.EqualTo(PlayState.Pause));

            Press(Key.Enter);
            Assert.That(viewModel.SequencerPlayState, Is.EqualTo(PlayState.Stop));
        }
    }
}
