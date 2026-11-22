using System;
using System.IO;
using System.Linq;
using Autofac;
using Avalonia;
using Avalonia.Headless;
using Avalonia.Headless.NUnit;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.VisualTree;
using Avalonia.Threading;
using NUnit.Framework;
using Sequencer.View.Control;
using Sequencer.ViewModel;
using Sequencer.Visual;
using Sequencer.Visual.Input;

namespace Sequencer.View.Tests.UiTests
{
    /// <summary>
    /// Drives the real main window with simulated input. Points are worked out from the controls' own
    /// positions, so the tests don't depend on the layout around them.
    /// </summary>
    [TestFixture]
    public class MainWindowTests
    {
        private IContainer container;
        private MainWindow window;
        private ISequencerNotes notes;
        private SequencerViewModel viewModel;
        private RawInputModifiers commandModifier;
        private RecordingNotePreviewer previewer;

        // A point in the piano roll, worked out from the editor's position (the layout above it can change).
        private Point EditorPoint;

        [SetUp]
        public void SetUp()
        {
            // Record previews instead of opening the real audio device.
            previewer = new RecordingNotePreviewer();
            container = App.BuildContainer(builder => builder.RegisterInstance(previewer).As<Domain.INotePreviewer>());
            window = container.Resolve<MainWindow>();
            notes = container.Resolve<ISequencerNotes>();
            viewModel = container.Resolve<SequencerViewModel>();
            commandModifier = container.Resolve<KeyboardStateProcessor>().CommandModifier == KeyModifiers.Meta
                ? RawInputModifiers.Meta
                : RawInputModifiers.Control;

            window.Show();
            Dispatcher.UIThread.RunJobs();
            EditorPoint = window.FindControl<SequencerControl>("Editor").TranslatePoint(new Point(64, 150), window) ?? default;
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
            window.CaptureRenderedFrame()?.Save(path, new Avalonia.Media.Imaging.PngBitmapEncoderOptions());
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
            Point menuCentre = EditorPoint + new Point(580, 70);
            window.MouseDown(menuCentre, MouseButton.Right);
            window.MouseMove(menuCentre + new Point(30, 0));

            string path = Path.Combine(TestContext.CurrentContext.WorkDirectory, "notes-and-menu.png");
            window.CaptureRenderedFrame()?.Save(path, new Avalonia.Media.Imaging.PngBitmapEncoderOptions());
            TestContext.Out.WriteLine($"Rendered frame: {path}");
            window.MouseUp(menuCentre + new Point(30, 0), MouseButton.Right);

            Assert.That(notes.AllNotes.Count(), Is.EqualTo(3));
        }

        /// <summary>
        /// Renders a representative screenshot (a melody and bassline, mid-playback) for the README.
        /// Run on demand: dotnet test --filter Screenshot_ForTheReadme
        /// </summary>
        [AvaloniaTest]
        [Explicit("Writes a screenshot for the docs.")]
        public void Screenshot_ForTheReadme()
        {
            window.RequestTune(TuneNamed("Korobeiniki"));

            // Select one note so the inspector has something to show, mid-way through the loop.
            Press(Key.V);
            IVisualNote highest = notes.AllNotes.OrderByDescending(n => NoteNumber(n.Pitch)).First();
            Click(NoteCentreInWindow(highest));
            System.Threading.Thread.Sleep(3000);
            Dispatcher.UIThread.RunJobs();

            string path = Path.Combine(TestContext.CurrentContext.WorkDirectory, "screenshot.png");
            window.CaptureRenderedFrame()?.Save(path, new Avalonia.Media.Imaging.PngBitmapEncoderOptions());
            TestContext.Out.WriteLine($"Screenshot: {path}");
            Press(Key.Enter);

            Assert.That(notes.AllNotes, Is.Not.Empty);
        }

        [AvaloniaTest]
        [Explicit("Writes a screenshot for the docs.")]
        public void Screenshot_Drums()
        {
            ShowDrums();
            window.RequestLoop(Domain.Drums.DrumLoopLibrary.All.Single(l => l.Name == "Boom Bap"));
            System.Threading.Thread.Sleep(1500);
            Dispatcher.UIThread.RunJobs();

            string path = Path.Combine(TestContext.CurrentContext.WorkDirectory, "drums.png");
            window.CaptureRenderedFrame()?.Save(path, new Avalonia.Media.Imaging.PngBitmapEncoderOptions());
            TestContext.Out.WriteLine($"Screenshot: {path}");
            Press(Key.Enter);
        }

        [AvaloniaTest]
        [Explicit("Writes a screenshot for the docs.")]
        public void Screenshot_Arrangement()
        {
            // Korobeiniki (A) twice, then Ode to Joy (B) twice, over a disco beat with a rock fill in section 3.
            window.RequestTune(TuneNamed("Korobeiniki"));
            window.RepeatSelectedClip();
            window.CreateClipAt(SynthTrack, 2);
            window.RequestTune(TuneNamed("Ode to Joy"));
            window.RepeatSelectedClip();

            window.Select(DrumTrack, 0);
            window.RequestLoop(Domain.Drums.DrumLoopLibrary.All.Single(l => l.Name == "Disco"));
            window.RepeatSelectedClip();
            window.CreateClipAt(DrumTrack, 2);
            window.RequestLoop(Domain.Drums.DrumLoopLibrary.All.Single(l => l.Name == "Rock Beat"));
            Song.Place(DrumTrack, 3, DrumTrack.ClipAt(0));
            Press(Key.Enter);

            window.Select(SynthTrack, 2);
            window.FindControl<ToggleButton>("SongModeButton").IsChecked = true;
            viewModel.BeatsPerMinute = 120;
            container.Resolve<Midi.IInternalClock>().Ticks = 2 * 1536 + 400;
            Press(Key.Space);
            System.Threading.Thread.Sleep(300);
            Dispatcher.UIThread.RunJobs();

            string path = Path.Combine(TestContext.CurrentContext.WorkDirectory, "arrangement.png");
            window.CaptureRenderedFrame()?.Save(path, new Avalonia.Media.Imaging.PngBitmapEncoderOptions());
            TestContext.Out.WriteLine($"Screenshot: {path}");
            Press(Key.Enter);
        }

        [AvaloniaTest]
        [Explicit("Writes a screenshot for the docs.")]
        public void Screenshot_Synth()
        {
            window.Height = 1400;
            window.ApplyPreset(Domain.Settings.SynthPresets.All.Single(p => p.Name == "Acid Bass"));
            window.FindControl<Expander>("FilterEnvelopeSection").IsExpanded = true;
            window.FindControl<Expander>("LfoSection").IsExpanded = true;
            Dispatcher.UIThread.RunJobs();

            string path = Path.Combine(TestContext.CurrentContext.WorkDirectory, "synth.png");
            window.CaptureRenderedFrame()?.Save(path, new Avalonia.Media.Imaging.PngBitmapEncoderOptions());
            TestContext.Out.WriteLine($"Screenshot: {path}");
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

        private int TotalTicks(Domain.IPosition position) => position.TotalTicks(Domain.TimeSignature.FourFour, 96);

        private IVisualNote CreateLongNote()
        {
            // About 60 px wide: several sixteenth-note cells (each about 14 px).
            window.MouseDown(EditorPoint, MouseButton.Left);
            window.MouseMove(EditorPoint + new Point(60, 0));
            window.MouseUp(EditorPoint + new Point(60, 0), MouseButton.Left);
            Dispatcher.UIThread.RunJobs();
            return notes.AllNotes.Single();
        }

        [AvaloniaTest]
        public void SelectMode_DraggingANote_MovesItWithoutChangingItsLength()
        {
            IVisualNote note = CreateLongNote();
            int start = TotalTicks(note.StartPosition);
            int length = TotalTicks(note.EndPosition) - start;
            viewModel.NoteAction = NoteAction.Select;

            // Grab the middle of the note and drag it right, holding Shift to keep the pitch.
            Point grab = EditorPoint + new Point(25, 0);
            window.MouseDown(grab, MouseButton.Left, RawInputModifiers.Shift);
            window.MouseMove(grab + new Point(56, 0), RawInputModifiers.Shift);
            window.MouseUp(grab + new Point(56, 0), MouseButton.Left, RawInputModifiers.Shift);

            Assert.That(TotalTicks(note.StartPosition), Is.GreaterThan(start));
            Assert.That(TotalTicks(note.EndPosition) - TotalTicks(note.StartPosition), Is.EqualTo(length));
        }

        [AvaloniaTest]
        public void SelectMode_DraggingANotesEnd_ResizesIt()
        {
            Click(EditorPoint);
            IVisualNote note = notes.AllNotes.Single();
            int lengthBefore = TotalTicks(note.EndPosition) - TotalTicks(note.StartPosition);
            viewModel.NoteAction = NoteAction.Select;

            // Grab the note's right edge.
            Point edge = NoteEdgeInWindow(note) - new Point(1, 0);
            window.MouseDown(edge, MouseButton.Left);
            window.MouseMove(edge + new Point(56, 0));
            window.MouseUp(edge + new Point(56, 0), MouseButton.Left);

            Assert.That(TotalTicks(note.EndPosition) - TotalTicks(note.StartPosition), Is.GreaterThan(lengthBefore));
        }

        [AvaloniaTest]
        public void SelectMode_DraggingAShortNotesMiddle_MovesItInsteadOfResizing()
        {
            Click(EditorPoint);
            IVisualNote note = notes.AllNotes.Single();
            int length = TotalTicks(note.EndPosition) - TotalTicks(note.StartPosition);
            string pitch = note.Pitch.ToString();
            viewModel.NoteAction = NoteAction.Select;

            Point middle = NoteEdgeInWindow(note) - new Point(EditorWidth() / 64 / 2, 0);
            window.MouseDown(middle, MouseButton.Left);
            window.MouseMove(middle + new Point(0, -30));
            window.MouseUp(middle + new Point(0, -30), MouseButton.Left);

            Assert.That(TotalTicks(note.EndPosition) - TotalTicks(note.StartPosition), Is.EqualTo(length));
            Assert.That(note.Pitch.ToString(), Is.Not.EqualTo(pitch));
        }

        [AvaloniaTest]
        public void ClickingTheRightHalfOfAShortNote_SelectsIt()
        {
            Click(EditorPoint);
            IVisualNote note = notes.AllNotes.Single();
            Press(Key.Escape);
            viewModel.NoteAction = NoteAction.Select;

            Click(NoteEdgeInWindow(note) - new Point(3, 0));

            Assert.That(note.NoteState, Is.EqualTo(NoteState.Selected));
        }

        private double EditorWidth() => window.FindControl<SequencerControl>("Editor").Bounds.Width;

        private double LaneHeight() => window.FindControl<SequencerControl>("Editor").Bounds.Height / 64;

        /// <summary>
        /// The middle of a key on the piano keyboard, counting lanes from the top (the highest note).
        /// </summary>
        private Point KeyInWindow(int lane)
        {
            var keyboard = window.FindControl<PianoKeyboardControl>("Keyboard");
            var inKeyboard = new Point(keyboard.Bounds.Width / 2, LaneHeight() * (lane + 0.5));
            return keyboard.TranslatePoint(inKeyboard, window) ?? default;
        }

        /// <summary>
        /// The window coordinates of the middle of a note: the centre of its first sixteenth and its lane.
        /// </summary>
        private Point NoteCentreInWindow(IVisualNote note)
        {
            var editor = window.FindControl<SequencerControl>("Editor");
            var settings = container.Resolve<Domain.Settings.SequencerSettings>();
            int lane = settings.TotalNotes - 1 - (NoteNumber(note.Pitch) - NoteNumber(settings.LowestPitch));
            double x = TotalTicks(note.StartPosition) * editor.Bounds.Width / (16 * 96) + EditorWidth() / 64 / 2;
            return editor.TranslatePoint(new Point(x, LaneHeight() * (lane + 0.5)), window) ?? default;
        }

        /// <summary>
        /// The window coordinates of the note's right edge, halfway down its lane.
        /// </summary>
        private Point NoteEdgeInWindow(IVisualNote note)
        {
            var editor = window.FindControl<SequencerControl>("Editor");
            double ticksAcross = 16 * 96;
            double x = TotalTicks(note.EndPosition) * editor.Bounds.Width / ticksAcross;
            Point origin = editor.TranslatePoint(new Point(0, 0), window) ?? default;
            return new Point(origin.X + x, EditorPoint.Y);
        }

        [AvaloniaTest]
        public void DeleteMode_ClickingANote_DeletesIt()
        {
            Click(EditorPoint);
            viewModel.NoteAction = NoteAction.Delete;

            Click(EditorPoint);

            Assert.That(notes.AllNotes, Is.Empty);
        }

        [AvaloniaTest]
        public void LeftAndRightArrows_MoveTheSelectedNoteBySixteenths()
        {
            Click(EditorPoint);
            IVisualNote note = notes.AllNotes.Single();
            int start = TotalTicks(note.StartPosition);

            Press(Key.Right);
            Assert.That(TotalTicks(note.StartPosition), Is.EqualTo(start + 24));

            Press(Key.Left);
            Press(Key.Left);
            Assert.That(TotalTicks(note.StartPosition), Is.EqualTo(start - 24));
        }

        [AvaloniaTest]
        public void Playing_AdvancesThePlaybackPosition()
        {
            Press(Key.Space);
            System.Threading.Thread.Sleep(400);
            Dispatcher.UIThread.RunJobs();
            Domain.IPosition playing = viewModel.CurrentPosition;
            Press(Key.Enter);

            Assert.That(TotalTicks(playing), Is.GreaterThan(0));
            Assert.That(TotalTicks(viewModel.CurrentPosition), Is.EqualTo(0), "Stop returns to the start.");
        }

        [AvaloniaTest]
        public void TempoBox_IsBoundToTheViewModel()
        {
            var tempoBox = window.FindControl<NumericUpDown>("TempoBox");

            tempoBox.Value = 90;
            Assert.That(viewModel.BeatsPerMinute, Is.EqualTo(90));

            viewModel.BeatsPerMinute = 140;
            Assert.That(tempoBox.Value, Is.EqualTo(140));
        }

        [AvaloniaTest]
        public void TypingInTheTempoBox_DoesNotTriggerEditorShortcuts()
        {
            Click(EditorPoint);
            TextBox tempoText = window.FindControl<NumericUpDown>("TempoBox").GetVisualDescendants().OfType<TextBox>().First();
            tempoText.Focus();

            Press(Key.Back);
            Press(Key.Space);

            Assert.That(notes.AllNotes.Count(), Is.EqualTo(1), "Backspace edits the text, not the notes.");
            Assert.That(viewModel.SequencerPlayState, Is.EqualTo(PlayState.Stop), "Space types a space, not Play.");
        }

        [AvaloniaTest]
        public void SynthPanel_IsBoundToTheSynthSettings()
        {
            var settings = SynthTrack.Synth;

            window.FindControl<ToggleButton>("TriangleWaveButton").IsChecked = true;
            window.FindControl<Slider>("ReleaseMillisecondsSlider").Value = 750;
            window.FindControl<Slider>("VolumePercentSlider").Value = 30;

            Assert.That(settings.Waveform, Is.EqualTo(Domain.Settings.Waveform.Triangle));
            Assert.That(settings.ReleaseSeconds, Is.EqualTo(0.75));
            Assert.That(settings.Volume, Is.EqualTo(0.3));
        }

        [AvaloniaTest]
        public void CreatingANote_PreviewsIt()
        {
            Click(EditorPoint);

            Assert.That(previewer.Pitches, Is.EqualTo(new[] { notes.AllNotes.Single().Pitch.ToString() }));
        }

        [AvaloniaTest]
        public void ChangingANotesPitch_PreviewsTheNewPitch()
        {
            Click(EditorPoint);

            Press(Key.Up);
            Press(Key.Right);

            Assert.That(previewer.Pitches.Count, Is.EqualTo(2), "Moving in time does not preview.");
            Assert.That(previewer.Pitches.Last(), Is.EqualTo(notes.AllNotes.Single().Pitch.ToString()));
        }

        [AvaloniaTest]
        public void DraggingANoteUpAndDown_PreviewsEachNewPitch()
        {
            Click(EditorPoint);
            viewModel.NoteAction = NoteAction.Select;
            previewer.Pitches.Clear();

            // Grab the middle of the note and drag up three lanes, one at a time.
            Point grab = NoteCentreInWindow(notes.AllNotes.Single());
            double lane = LaneHeight();
            window.MouseDown(grab, MouseButton.Left);
            window.MouseMove(grab - new Point(0, lane));
            window.MouseMove(grab - new Point(0, lane * 2));
            window.MouseMove(grab - new Point(0, lane * 3));
            window.MouseUp(grab - new Point(0, lane * 3), MouseButton.Left);

            Assert.That(previewer.Pitches.Count, Is.EqualTo(3));
        }

        [AvaloniaTest]
        public void PreviewCheckbox_IsBoundToTheSetting()
        {
            window.FindControl<CheckBox>("PreviewOnEditBox").IsChecked = false;

            Assert.That(container.Resolve<Domain.Settings.EditingSettings>().PreviewOnEdit, Is.False);
        }

        private PianoKeyboardControl Keyboard => window.FindControl<PianoKeyboardControl>("Keyboard");

        private string PitchName(int noteNumber) => container.Resolve<Midi.IDigitalAudioProtocol>().CreatePitchFromProtocolNumber(noteNumber).ToString();

        private int NoteNumber(Domain.Pitch pitch) => container.Resolve<Midi.IDigitalAudioProtocol>().ProtocolNoteNumber(pitch);

        [AvaloniaTest]
        public void PressingAKey_HoldsItsNoteUntilReleased()
        {
            var settings = container.Resolve<Domain.Settings.SequencerSettings>();
            int highest = NoteNumber(settings.LowestPitch) + settings.TotalNotes - 1;

            window.MouseDown(KeyInWindow(0), MouseButton.Left);
            Assert.That(previewer.Held, Is.EqualTo(new[] { PitchName(highest) }));
            Assert.That(Keyboard.PressedNote, Is.EqualTo(highest));
            Assert.That(previewer.Released, Is.EqualTo(0));

            window.MouseUp(KeyInWindow(0), MouseButton.Left);
            Assert.That(previewer.Released, Is.EqualTo(1));
            Assert.That(Keyboard.PressedNote, Is.Null);
        }

        [AvaloniaTest]
        public void DraggingAcrossTheKeys_PlaysEachInTurn()
        {
            window.MouseDown(KeyInWindow(0), MouseButton.Left);
            window.MouseMove(KeyInWindow(1));
            window.MouseMove(KeyInWindow(2));
            window.MouseUp(KeyInWindow(2), MouseButton.Left);

            Assert.That(previewer.Held.Count, Is.EqualTo(3));
            Assert.That(previewer.Held.Distinct().Count(), Is.EqualTo(3));
            Assert.That(previewer.Released, Is.EqualTo(3), "Each key is released when the next is played.");
        }

        [AvaloniaTest]
        public void PlayingKeys_LightUpDuringPlayback()
        {
            // A note in the first cell of the grid, which sounds as soon as playback starts.
            Point firstCell = (window.FindControl<SequencerControl>("Editor").TranslatePoint(new Point(4, 0), window) ?? default)
                .WithY(EditorPoint.Y);
            Click(firstCell);
            int noteNumber = NoteNumber(notes.AllNotes.Single().Pitch);

            Press(Key.Space);
            System.Threading.Thread.Sleep(60);
            Dispatcher.UIThread.RunJobs();
            Assert.That(Keyboard.ActiveNotes, Is.EquivalentTo(new[] { noteNumber }));

            Press(Key.Enter);
            Assert.That(Keyboard.ActiveNotes, Is.Empty);
        }

        [AvaloniaTest]
        public void Ruler_LinesUpWithTheGrid()
        {
            var header = window.FindControl<SequencerHeaderControl>("Header");
            var editor = window.FindControl<SequencerControl>("Editor");

            Point headerOrigin = header.TranslatePoint(new Point(0, 0), window) ?? default;
            Point editorOrigin = editor.TranslatePoint(new Point(0, 0), window) ?? default;

            Assert.That(headerOrigin.X, Is.EqualTo(editorOrigin.X));
            Assert.That(header.Bounds.Width, Is.EqualTo(editor.Bounds.Width).Within(1));
        }

        [AvaloniaTest]
        public void LetterKeys_SwitchTools()
        {
            Press(Key.V);
            Assert.That(viewModel.NoteAction, Is.EqualTo(NoteAction.Select));

            Press(Key.E);
            Assert.That(viewModel.NoteAction, Is.EqualTo(NoteAction.Delete));

            Press(Key.D);
            Assert.That(viewModel.NoteAction, Is.EqualTo(NoteAction.Create));
        }

        [AvaloniaTest]
        public void ToolButtons_ShowAndChangeTheTool()
        {
            Assert.That(window.FindControl<ToggleButton>("DrawToolButton").IsChecked, Is.True);

            window.FindControl<ToggleButton>("EraseToolButton").IsChecked = true;

            Assert.That(viewModel.NoteAction, Is.EqualTo(NoteAction.Delete));
            Assert.That(window.FindControl<ToggleButton>("DrawToolButton").IsChecked, Is.False);
        }

        [AvaloniaTest]
        public void EmptyHint_ShowsOnlyOnAnEmptyGridWithTheDrawTool()
        {
            var hint = window.FindControl<Border>("EmptyHint");
            Assert.That(hint.IsVisible, Is.True);

            Click(EditorPoint);
            Assert.That(hint.IsVisible, Is.False, "Hidden once there is a note.");

            Press(Key.Back);
            Assert.That(hint.IsVisible, Is.True, "Back when the grid is empty again.");

            Press(Key.V);
            Assert.That(hint.IsVisible, Is.False, "Only for the Draw tool.");
        }

        [AvaloniaTest]
        public void Inspector_ShowsPositionsAndLengthMusically()
        {
            Click(EditorPoint);

            Assert.That(window.FindControl<TextBlock>("SelectedLength").Text, Is.EqualTo("1/16"));
            Assert.That(window.FindControl<TextBlock>("SelectedStart").Text, Does.Match(@"^\d+\.\d\.\d$"));
        }

        private static Domain.Tunes.Tune TuneNamed(string name) => Domain.Tunes.TuneLibrary.All.Single(t => t.Name == name);

        [AvaloniaTest]
        public void TunesMenu_ListsEveryTune()
        {
            var menu = (MenuFlyout) window.FindControl<Button>("TunesButton").Flyout;

            Assert.That(menu.Items.Count, Is.EqualTo(Domain.Tunes.TuneLibrary.All.Count));
        }

        [AvaloniaTest]
        public void LoadingATune_OnAnEmptyGrid_FillsItAndPlays()
        {
            Domain.Tunes.Tune ode = TuneNamed("Ode to Joy");

            window.RequestTune(ode);
            Dispatcher.UIThread.RunJobs();

            Assert.That(notes.AllNotes.Count(), Is.EqualTo(ode.ToTones(Domain.TimeSignature.FourFour, 96).Count));
            Assert.That(notes.SelectedNotes, Is.Empty, "Loaded notes start unselected.");
            Assert.That(viewModel.BeatsPerMinute, Is.EqualTo(ode.BeatsPerMinute));
            Assert.That(SynthTrack.Synth.Waveform, Is.EqualTo(ode.Waveform));
            Assert.That(viewModel.SequencerPlayState, Is.EqualTo(PlayState.Play));
            Assert.That(previewer.Pitches, Is.Empty, "Loading does not preview every note.");
            Press(Key.Enter);
        }

        [AvaloniaTest]
        public void LoadingATune_OverExistingNotes_AsksFirst()
        {
            Click(EditorPoint);

            window.RequestTune(TuneNamed("Jingle Bells"));
            Assert.That(window.ReplaceConfirmButton, Is.Not.Null, "Asks before replacing.");
            Assert.That(notes.AllNotes.Count(), Is.EqualTo(1), "Nothing replaced yet.");

            window.ReplaceConfirmButton.RaiseEvent(new Avalonia.Interactivity.RoutedEventArgs(Button.ClickEvent));
            Dispatcher.UIThread.RunJobs();

            Assert.That(notes.AllNotes.Count(), Is.GreaterThan(1));
            Press(Key.Enter);
        }

        [AvaloniaTest]
        public void LoadingATune_ThenCancelling_KeepsTheNotes()
        {
            Click(EditorPoint);

            window.RequestTune(TuneNamed("Jingle Bells"));
            var flyoutCancel = ((Panel) window.ReplaceConfirmButton.Parent).Children.OfType<Button>().First();
            flyoutCancel.RaiseEvent(new Avalonia.Interactivity.RoutedEventArgs(Button.ClickEvent));
            Dispatcher.UIThread.RunJobs();

            Assert.That(notes.AllNotes.Count(), Is.EqualTo(1));
            Assert.That(viewModel.SequencerPlayState, Is.EqualTo(PlayState.Stop));
        }

        private Domain.Arrangement.Song Song => container.Resolve<Domain.Arrangement.Song>();

        private Domain.Arrangement.Track SynthTrack => Song.Tracks.First(t => t.Kind == Domain.Arrangement.TrackKind.Synth);

        private Domain.Arrangement.Track DrumTrack => Song.Tracks.First(t => t.Kind == Domain.Arrangement.TrackKind.Drums);

        private Domain.Drums.DrumPattern DrumPattern => ((Domain.Arrangement.DrumClip) DrumTrack.ClipAt(0)).Pattern;

        private void ShowDrums()
        {
            window.Select(DrumTrack, 0);
            Dispatcher.UIThread.RunJobs();
        }

        /// <summary>
        /// The middle of a step in the drum grid, in window coordinates.
        /// </summary>
        private Point DrumStepInWindow(Domain.Drums.DrumSound sound, int step)
        {
            var editor = window.FindControl<DrumEditorControl>("DrumEditor");
            double cell = (editor.Bounds.Width - DrumEditorControl.LabelWidth) / Domain.Drums.DrumPattern.StepCount;
            int row = System.Array.IndexOf(Domain.Drums.DrumPattern.AllSounds, sound);
            var point = new Point(DrumEditorControl.LabelWidth + (step + 0.5) * cell, (row + 0.5) * DrumEditorControl.RowHeight);
            return editor.TranslatePoint(point, window) ?? default;
        }

        [AvaloniaTest]
        public void DrumTrack_ShowsTheDrumGridInsteadOfThePianoRoll()
        {
            ShowDrums();

            Assert.That(window.FindControl<DrumEditorControl>("DrumEditor").IsEffectivelyVisible, Is.True);
            Assert.That(window.FindControl<ScrollViewer>("EditorScrollViewer").IsVisible, Is.False);
            Assert.That(window.FindControl<Button>("LoopsButton").IsVisible, Is.True);
            Assert.That(window.FindControl<Button>("TunesButton").IsVisible, Is.False);
            Assert.That(window.FindControl<StackPanel>("DrumKitPanel").IsVisible, Is.True);
        }

        [AvaloniaTest]
        public void ClickingAStep_TogglesItAndPreviewsIt()
        {
            ShowDrums();

            Click(DrumStepInWindow(Domain.Drums.DrumSound.Snare, 4));
            Assert.That(DrumPattern.VelocityAt(Domain.Drums.DrumSound.Snare, 4), Is.EqualTo(Domain.Drums.DrumPattern.NormalVelocity));
            Assert.That(previewer.DrumPreviews, Is.EqualTo(new[] { Domain.Drums.DrumSound.Snare }));

            Click(DrumStepInWindow(Domain.Drums.DrumSound.Snare, 4));
            Assert.That(DrumPattern.IsHit(Domain.Drums.DrumSound.Snare, 4), Is.False);
        }

        [AvaloniaTest]
        public void AltClickingAStep_AddsAnAccent()
        {
            ShowDrums();

            Click(DrumStepInWindow(Domain.Drums.DrumSound.Kick, 0), modifiers: RawInputModifiers.Alt);

            Assert.That(DrumPattern.VelocityAt(Domain.Drums.DrumSound.Kick, 0), Is.EqualTo(Domain.Drums.DrumPattern.AccentVelocity));
        }

        [AvaloniaTest]
        public void DraggingAcrossSteps_PaintsHits()
        {
            ShowDrums();

            window.MouseDown(DrumStepInWindow(Domain.Drums.DrumSound.ClosedHat, 0), MouseButton.Left);
            for (var step = 1; step < 8; step++)
            {
                window.MouseMove(DrumStepInWindow(Domain.Drums.DrumSound.ClosedHat, step));
            }

            window.MouseUp(DrumStepInWindow(Domain.Drums.DrumSound.ClosedHat, 7), MouseButton.Left);

            Assert.That(Enumerable.Range(0, 8).All(step => DrumPattern.IsHit(Domain.Drums.DrumSound.ClosedHat, step)), Is.True);
            Assert.That(previewer.DrumPreviews.Count, Is.EqualTo(1), "Only the first hit of a drag is previewed.");
        }

        [AvaloniaTest]
        public void ClickingASoundsName_PlaysIt()
        {
            ShowDrums();
            var editor = window.FindControl<DrumEditorControl>("DrumEditor");
            int row = System.Array.IndexOf(Domain.Drums.DrumPattern.AllSounds, Domain.Drums.DrumSound.Clap);

            Click(editor.TranslatePoint(new Point(20, (row + 0.5) * DrumEditorControl.RowHeight), window) ?? default);

            Assert.That(previewer.DrumsPlayed, Is.EqualTo(new[] { Domain.Drums.DrumSound.Clap }));
            Assert.That(DrumPattern.IsEmpty, Is.True);
        }

        [AvaloniaTest]
        public void LoadingALoop_FillsThePatternAndPlays()
        {
            Domain.Drums.DrumLoop loop = Domain.Drums.DrumLoopLibrary.All.First();
            ShowDrums();

            window.RequestLoop(loop);

            Assert.That(DrumPattern.HitCount, Is.EqualTo(loop.ToPattern().HitCount));
            Assert.That(viewModel.BeatsPerMinute, Is.EqualTo(loop.BeatsPerMinute));
            Assert.That(viewModel.SequencerPlayState, Is.EqualTo(PlayState.Play));
            Assert.That(window.FindControl<TextBlock>("DrumEmptyHint").IsVisible, Is.False);
            Press(Key.Enter);
        }

        [AvaloniaTest]
        public void LoadingALoop_OverAPattern_AsksFirst()
        {
            ShowDrums();
            Click(DrumStepInWindow(Domain.Drums.DrumSound.Tom, 3));

            window.RequestLoop(Domain.Drums.DrumLoopLibrary.All.Last());

            Assert.That(window.ReplaceConfirmButton, Is.Not.Null);
            Assert.That(DrumPattern.HitCount, Is.EqualTo(1));
        }

        [AvaloniaTest]
        public void LoopsMenu_ListsEveryLoop()
        {
            var menu = (MenuFlyout) window.FindControl<Button>("LoopsButton").Flyout;

            Assert.That(menu.Items.Count, Is.EqualTo(Domain.Drums.DrumLoopLibrary.All.Count));
        }

        [AvaloniaTest]
        public void NoteShortcuts_DoNothingOnTheDrumTrack()
        {
            Click(EditorPoint);
            ShowDrums();

            Press(Key.Back);

            Assert.That(notes.AllNotes.Count(), Is.EqualTo(1), "The selected synth note isn't on screen, so it isn't deleted.");
        }

        [AvaloniaTest]
        public void Playback_HighlightsTheCurrentStep()
        {
            ShowDrums();
            var editor = window.FindControl<DrumEditorControl>("DrumEditor");
            Assert.That(editor.CurrentStep, Is.EqualTo(-1));

            Press(Key.Space);
            System.Threading.Thread.Sleep(300);
            Dispatcher.UIThread.RunJobs();
            Assert.That(editor.CurrentStep, Is.GreaterThanOrEqualTo(0));

            Press(Key.Enter);
            Assert.That(editor.CurrentStep, Is.EqualTo(-1));
        }

        private ArrangementControl ArrangementView => window.FindControl<ArrangementControl>("Arrangement");

        private Point CellInWindow(Domain.Arrangement.Track track, int section) =>
            ArrangementView.TranslatePoint(ArrangementView.CellCentre(Song.Tracks.ToList().IndexOf(track), section), window) ?? default;

        private Domain.Arrangement.ArrangementState Arrangement => container.Resolve<Domain.Arrangement.ArrangementState>();

        [AvaloniaTest]
        public void TheDefaultSong_HasASynthAndADrumTrack_WithTheSynthOpen()
        {
            Assert.That(Song.Tracks.Select(t => t.Kind), Is.EqualTo(new[] { Domain.Arrangement.TrackKind.Synth, Domain.Arrangement.TrackKind.Drums }));
            Assert.That(Arrangement.SelectedTrack, Is.SameAs(SynthTrack));
            Assert.That(window.FindControl<TextBlock>("ClipTitle").Text, Is.EqualTo("Synth"));
        }

        [AvaloniaTest]
        public void ClickingACell_OpensItsClip()
        {
            Click(CellInWindow(DrumTrack, 0));

            Assert.That(Arrangement.SelectedTrack, Is.SameAs(DrumTrack));
            Assert.That(viewModel.IsDrumTrack, Is.True);
            Assert.That(window.FindControl<DrumEditorControl>("DrumEditor").Pattern, Is.SameAs(DrumPattern));
        }

        [AvaloniaTest]
        public void ClickingAnEmptyCell_OffersANewClip_AndLoopsThatSection()
        {
            Click(CellInWindow(SynthTrack, 2));

            Assert.That(window.FindControl<Border>("NoClipPanel").IsVisible, Is.True);
            Assert.That(Arrangement.LoopSection, Is.EqualTo(2), "Section mode loops what you're editing.");
            Assert.That(window.FindControl<SequencerHeaderControl>("Header").FirstBarNumber, Is.EqualTo(9), "Section 3 starts at bar 9.");

            window.FindControl<Button>("NewClipButton").RaiseEvent(new Avalonia.Interactivity.RoutedEventArgs(Button.ClickEvent));

            Assert.That(SynthTrack.ClipAt(2), Is.Not.Null);
            Assert.That(window.FindControl<Border>("NoClipPanel").IsVisible, Is.False);
        }

        [AvaloniaTest]
        public void DoubleClickingAnEmptyCell_MakesAClip()
        {
            Point cell = CellInWindow(DrumTrack, 3);
            window.MouseDown(cell, MouseButton.Left);
            window.MouseUp(cell, MouseButton.Left);
            window.MouseDown(cell, MouseButton.Left);
            window.MouseUp(cell, MouseButton.Left);
            Dispatcher.UIThread.RunJobs();

            Assert.That(DrumTrack.ClipAt(3), Is.InstanceOf<Domain.Arrangement.DrumClip>());
            Assert.That(DrumTrack.ClipAt(3).Name, Is.EqualTo("B"));
        }

        [AvaloniaTest]
        public void CommandD_RepeatsTheClipInTheNextSection_AsTheSameClip()
        {
            Click(EditorPoint);

            Press(Key.D, commandModifier);

            Assert.That(SynthTrack.ClipAt(1), Is.SameAs(SynthTrack.ClipAt(0)));
            Assert.That(Arrangement.SelectedSection, Is.EqualTo(1));
            Assert.That(notes.AllNotes.Count(), Is.EqualTo(1), "The repeated clip opens with its note.");
        }

        [AvaloniaTest]
        public void ARepeatedClip_EditedInOneSection_ChangesInBoth()
        {
            Press(Key.D, commandModifier);
            Click(EditorPoint);

            Click(CellInWindow(SynthTrack, 0));

            Assert.That(notes.AllNotes.Count(), Is.EqualTo(1));
        }

        [AvaloniaTest]
        public void Notes_SurviveSwitchingToAnotherClipAndBack()
        {
            Click(EditorPoint);
            Click(EditorPoint + new Point(100, 30));

            Click(CellInWindow(DrumTrack, 0));
            Click(CellInWindow(SynthTrack, 0));

            Assert.That(notes.AllNotes.Count(), Is.EqualTo(2));
            Assert.That(((Domain.Arrangement.NoteClip) SynthTrack.ClipAt(0)).Tones.Count, Is.EqualTo(2));
        }

        [AvaloniaTest]
        public void TheMuteButton_MutesItsTrack()
        {
            Click(ArrangementView.TranslatePoint(ArrangementView.MuteButtonCentre(1), window) ?? default);

            Assert.That(DrumTrack.IsMuted, Is.True);
            Assert.That(Arrangement.SelectedTrack, Is.SameAs(SynthTrack), "Muting doesn't change what's being edited.");
        }

        [AvaloniaTest]
        public void AddTrackButtons_AddATrackWithAClip_AndOpenIt()
        {
            Click(ArrangementView.TranslatePoint(ArrangementView.AddTrackButtonCentre(Domain.Arrangement.TrackKind.Synth), window) ?? default);

            Assert.That(Song.Tracks.Count, Is.EqualTo(3));
            Domain.Arrangement.Track added = Song.Tracks.Last();
            Assert.That(added.Name, Is.EqualTo("Synth 2"));
            Assert.That(Arrangement.SelectedTrack, Is.SameAs(added));
            Assert.That(added.ClipAt(0), Is.Not.Null);
        }

        [AvaloniaTest]
        public void SongMode_LoopsTheWholeSong_AndSectionModeOneSection()
        {
            var clock = container.Resolve<Midi.ISequencerClock>();
            Press(Key.D, commandModifier);
            Press(Key.D, commandModifier);

            window.FindControl<ToggleButton>("SongModeButton").IsChecked = true;
            Assert.That(clock.LoopLengthTicks, Is.EqualTo(3 * 1536), "Up to the last clip: three sections.");

            window.FindControl<ToggleButton>("SectionModeButton").IsChecked = true;
            Assert.That(clock.LoopLengthTicks, Is.EqualTo(1536));
        }

        [AvaloniaTest]
        public void ThePositionReadout_ShowsTheBarInTheSong()
        {
            Click(CellInWindow(SynthTrack, 1));

            Assert.That(window.FindControl<TextBlock>("PositionReadout").Text, Is.EqualTo("5.1.1"));
        }

        [AvaloniaTest]
        public void LoadingATune_IntoAnEmptySection_MakesAClipForIt()
        {
            Click(CellInWindow(SynthTrack, 4));

            window.RequestTune(TuneNamed("Ode to Joy"));

            Assert.That(SynthTrack.ClipAt(4), Is.Not.Null);
            Assert.That(notes.AllNotes, Is.Not.Empty);
            Press(Key.Enter);
        }

        [AvaloniaTest]
        public void ThePlayhead_ShowsInTheArrangementWhilePlaying()
        {
            Assert.That(ArrangementView.PlayheadTick, Is.EqualTo(-1));

            Press(Key.Space);
            System.Threading.Thread.Sleep(200);
            Dispatcher.UIThread.RunJobs();
            Assert.That(ArrangementView.PlayheadTick, Is.GreaterThanOrEqualTo(0));

            Press(Key.Enter);
            Assert.That(ArrangementView.PlayheadTick, Is.EqualTo(-1));
        }

        [AvaloniaTest]
        public void PresetsMenu_ListsEveryPreset()
        {
            var menu = (MenuFlyout) window.FindControl<Button>("PresetsButton").Flyout;

            Assert.That(menu.Items.Count, Is.EqualTo(Domain.Settings.SynthPresets.All.Count));
        }

        [AvaloniaTest]
        public void ApplyingAPreset_ChangesTheTracksPatch_AndThePanel()
        {
            window.ApplyPreset(Domain.Settings.SynthPresets.All.Single(p => p.Name == "Acid Bass"), previewer);

            Assert.That(SynthTrack.Synth.Resonance, Is.EqualTo(0.8));
            Assert.That(window.FindControl<Slider>("ResonancePercentSlider").Value, Is.EqualTo(80));
            Assert.That(previewer.Pitches.Count, Is.EqualTo(1), "Plays a note so the preset can be heard.");
        }

        [AvaloniaTest]
        public void FilterTypeButtons_SetTheFilter()
        {
            window.FindControl<ToggleButton>("BandPassButton").IsChecked = true;

            Assert.That(SynthTrack.Synth.FilterType, Is.EqualTo(Domain.Settings.FilterType.BandPass));
            Assert.That(window.FindControl<ToggleButton>("LowPassButton").IsChecked, Is.False);
        }

        [AvaloniaTest]
        public void EachSynthTrack_HasItsOwnPatch()
        {
            window.ApplyPreset(Domain.Settings.SynthPresets.All.Single(p => p.Name == "Lead"));
            window.AddTrack(Domain.Arrangement.TrackKind.Synth);

            Assert.That(window.FindControl<Slider>("ResonancePercentSlider").Value, Is.EqualTo(0), "The new track starts from Init.");

            window.Select(SynthTrack, 0);
            Assert.That(window.FindControl<Slider>("ResonancePercentSlider").Value, Is.EqualTo(25), "The first track kept its Lead patch.");
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

        private sealed class RecordingNotePreviewer : Domain.INotePreviewer
        {
            public System.Collections.Generic.List<string> Pitches { get; } = new System.Collections.Generic.List<string>();

            public System.Collections.Generic.List<string> Held { get; } = new System.Collections.Generic.List<string>();

            public int Released { get; private set; }

            public void Preview(Domain.Tone tone) => Pitches.Add(tone.Pitch.ToString());

            public System.Collections.Generic.List<Domain.Drums.DrumSound> DrumPreviews { get; } = new System.Collections.Generic.List<Domain.Drums.DrumSound>();

            public System.Collections.Generic.List<Domain.Drums.DrumSound> DrumsPlayed { get; } = new System.Collections.Generic.List<Domain.Drums.DrumSound>();

            public void PreviewDrum(Domain.Drums.DrumSound sound, byte velocity) => DrumPreviews.Add(sound);

            public void PlayDrum(Domain.Drums.DrumSound sound, byte velocity) => DrumsPlayed.Add(sound);

            public IDisposable Hold(Domain.Tone tone)
            {
                Held.Add(tone.Pitch.ToString());
                return new Release(() => Released++);
            }

            private sealed class Release : IDisposable
            {
                private readonly Action onDispose;

                public Release(Action onDispose) => this.onDispose = onDispose;

                public void Dispose() => onDispose();
            }
        }
    }
}
