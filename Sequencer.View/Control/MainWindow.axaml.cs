using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Avalonia;
using Avalonia.Controls.Primitives;
using Avalonia.Layout;
using Avalonia.Media;
using Sequencer.Domain.Arrangement;
using Track = Sequencer.Domain.Arrangement.Track;
using Sequencer.Domain.Drums;
using Sequencer.Domain.Tunes;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using JetBrains.Annotations;
using log4net;
using Sequencer.Domain;
using Sequencer.Domain.Settings;
using Sequencer.Midi;
using Sequencer.Utilities;
using Sequencer.ViewModel;
using Sequencer.Visual;
using Sequencer.Visual.Input;

namespace Sequencer.View.Control
{
    public partial class MainWindow : Window
    {
        [NotNull] private static readonly ILog Log = LogExtensions.GetLoggerSafe(typeof(MainWindow));

        private readonly KeyboardStateProcessor keyboardState;
        private readonly MouseOperator mouseOperator;
        private readonly SequencerViewModel viewModel;
        private readonly ISequencerNotes notes;
        private readonly SynthViewModel synthViewModel;
        private readonly IDigitalAudioProtocol protocol;
        private readonly SequencerSettings settings;
        private readonly Song song;
        private readonly ArrangementState arrangement;
        private readonly DrumKitViewModel drumKitViewModel;

        // The note clip open in the piano roll, which plays from the editor's notes until it's closed.
        [CanBeNull] private NoteClip openNoteClip;
        [CanBeNull] private DrumPattern openDrumPattern;

        // The button that opened the radial menu: right, or left with Ctrl on macOS.
        private MouseButton? contextMenuButton;

        /// <summary>
        /// For the XAML designer only.
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();
        }

        public MainWindow([NotNull] SequencerViewModel viewModel, [NotNull] ISequencerNotes notes,
            [NotNull] IPitchAndPositionCalculator pitchAndPositionCalculator, [NotNull] IDigitalAudioProtocol protocol,
            [NotNull] SequencerSettings settings, [NotNull] KeyboardStateProcessor keyboardState, [NotNull] MouseOperator mouseOperator,
            [NotNull] SynthViewModel synthViewModel, [NotNull] INotePreviewer notePreviewer,
            [NotNull] DrumKitViewModel drumKitViewModel, [NotNull] Song song, [NotNull] ArrangementState arrangement)
        {
            InitializeComponent();

            this.viewModel = viewModel;
            this.notes = notes;
            this.synthViewModel = synthViewModel;
            this.protocol = protocol;
            this.settings = settings;
            this.song = song;
            this.arrangement = arrangement;
            this.drumKitViewModel = drumKitViewModel;
            this.keyboardState = keyboardState;
            this.mouseOperator = mouseOperator;
            DataContext = viewModel;
            SynthPanel.DataContext = synthViewModel;
            DrumVolumeRow.DataContext = drumKitViewModel;

            Header.Initialise(pitchAndPositionCalculator, protocol, settings);
            Editor.Initialise(notes, pitchAndPositionCalculator, protocol, settings, keyboardState, mouseOperator, keyboardState.CommandModifier, notePreviewer);

            Keyboard.Initialise(protocol, settings, notePreviewer);
            viewModel.PropertyChanged += OnViewModelPropertyChanged;
            notes.NotesChanged += (sender, args) => UpdateEmptyHint();
            UpdateEmptyHint();
            BuildTunesMenu();
            BuildLoopsMenu();

            DrumEditor.PreviewSound = notePreviewer.PreviewDrum;
            DrumEditor.AuditionSound = notePreviewer.PlayDrum;

            Arrangement.Initialise(song, arrangement);
            synthViewModel.PropertyChanged += (sender, args) => Arrangement.InvalidateVisual();
            Arrangement.CellClicked += (sender, args) => Select(args.Track, args.Section);
            Arrangement.CellDoubleClicked += (sender, args) => CreateClipAt(args.Track, args.Section);
            Arrangement.CellContextRequested += (sender, args) => ShowCellMenu(args);
            Arrangement.MuteToggled += (sender, track) => ToggleMute(track);
            Arrangement.AddTrackRequested += (sender, kind) => AddTrack(kind);
            NewClipButton.Click += (sender, args) => CreateClipAt(arrangement.SelectedTrack, arrangement.SelectedSection);

            // Redraw the arrangement's clip miniatures after edits in the piano roll.
            Editor.PointerReleased += (sender, args) => song.NotifyClipEdited();
            notes.NotesChanged += (sender, args) => song.NotifyClipEdited();

            Select(song.Tracks.First(), 0);

            Editor.PointerPressed += OnEditorPointerPressed;
            Editor.PointerMoved += OnEditorPointerMoved;
            Editor.PointerReleased += OnEditorPointerReleased;
            Editor.PointerCaptureLost += (sender, args) => ResetPointerState();

            // Tunnel, so shortcuts reach the editor before controls such as the scroll viewer consume them.
            AddHandler(KeyDownEvent, OnKeyDown, RoutingStrategies.Tunnel);
            AddHandler(KeyUpEvent, (sender, args) => keyboardState.CurrentModifiers = args.KeyModifiers, RoutingStrategies.Tunnel);

            Log.Info("Main window loaded");
        }

        /// <summary>
        /// The confirmation shown before a tune replaces existing notes, while it is open. For tests.
        /// </summary>
        [CanBeNull]
        internal Button ReplaceConfirmButton { get; private set; }

        /// <summary>
        /// Loads a tune, first asking before it replaces notes (there is no undo).
        /// </summary>
        internal void RequestTune([NotNull] Tune tune)
        {
            EnsureClip(TrackKind.Synth);
            int existing = notes.AllNotes.Count();

            if (existing == 0)
            {
                LoadTune(tune);
                return;
            }

            ConfirmReplace($"Replace your {existing} note{(existing == 1 ? "" : "s")} with {tune.Name}?", TunesButton, () => LoadTune(tune));
        }

        /// <summary>
        /// Loads a drum loop, first asking before it replaces a pattern (there is no undo).
        /// </summary>
        internal void RequestLoop([NotNull] DrumLoop loop)
        {
            EnsureClip(TrackKind.Drums);
            DrumPattern pattern = openDrumPattern;

            if (pattern == null || pattern.IsEmpty)
            {
                LoadLoop(loop);
                return;
            }

            ConfirmReplace($"Replace your {pattern.HitCount}-hit pattern with {loop.Name}?", LoopsButton, () => LoadLoop(loop));
        }

        /// <summary>
        /// Replaces the drum pattern with the loop, sets its tempo, and plays it.
        /// </summary>
        internal void LoadLoop([NotNull] DrumLoop loop)
        {
            if (viewModel.StopSequencer.CanExecute(null))
            {
                viewModel.StopSequencer.Execute(null);
            }

            EnsureClip(TrackKind.Drums);
            openDrumPattern?.CopyFrom(loop.ToPattern());
            viewModel.BeatsPerMinute = loop.BeatsPerMinute;
            viewModel.PlaySequencer.Execute(null);
            Log.Info($"Loaded drum loop {loop.Name}");
        }

        /// <summary>
        /// The track and section being edited.
        /// </summary>
        [CanBeNull]
        internal Clip SelectedClip => arrangement.SelectedTrack?.ClipAt(arrangement.SelectedSection);

        /// <summary>
        /// Opens a cell for editing: its clip in the piano roll or drum grid, and its track's instrument in the
        /// side panel. In section mode, that section loops.
        /// </summary>
        internal void Select([NotNull] Track track, int section)
        {
            CloseOpenClip();

            arrangement.SelectedTrack = track;
            arrangement.SelectedSection = section;

            if (!viewModel.IsSongMode)
            {
                arrangement.LoopSection = section;
            }

            viewModel.IsDrumTrack = track.Kind == TrackKind.Drums;
            Clip clip = track.ClipAt(section);

            if (track.Kind == TrackKind.Synth)
            {
                synthViewModel.Settings = track.Synth;
                var noteClip = clip as NoteClip;
                Editor.ReplaceNotes(noteClip?.Tones ?? Array.Empty<Tone>());

                if (noteClip != null)
                {
                    // From now on playback reads the editor's notes, so edits are heard straight away.
                    noteClip.Open(notes as ISequencedNotes ?? throw new InvalidOperationException("The editor's notes can't be played."));
                    openNoteClip = noteClip;
                }
            }
            else
            {
                drumKitViewModel.Settings = track.Drums;
                openDrumPattern = (clip as DrumClip)?.Pattern;

                if (openDrumPattern != null)
                {
                    openDrumPattern.Changed += OnOpenDrumPatternChanged;
                }

                DrumEditor.Pattern = openDrumPattern;
                DrumEmptyHint.IsVisible = openDrumPattern?.IsEmpty ?? false;
            }

            NoClipPanel.IsVisible = clip == null;
            NoClipTitle.Text = $"No {(track.Kind == TrackKind.Synth ? "synth" : "drum")} clip in section {section + 1}";
            Header.FirstBarNumber = section * 4 + 1;
            ClipColour.Fill = new SolidColorBrush(ArrangementControl.TrackColours[track.ColourIndex % ArrangementControl.TrackColours.Length]);
            ClipTitle.Text = track.Name;
            ClipDetail.Text = clip == null ? $"section {section + 1} · empty" : $"clip {clip.Name} · bars {section * 4 + 1}–{section * 4 + 4}";
            UpdateEmptyHint();
            Arrangement.InvalidateVisual();
        }

        /// <summary>
        /// Makes a new, empty clip in a cell and opens it.
        /// </summary>
        internal void CreateClipAt([CanBeNull] Track track, int section)
        {
            if (track == null)
            {
                return;
            }

            CloseOpenClip();
            song.Place(track, section, song.CreateClip(track));
            Select(track, section);
        }

        /// <summary>
        /// Repeats the selected clip in the next section (the same clip, so editing either changes both).
        /// </summary>
        internal void RepeatSelectedClip()
        {
            Track track = arrangement.SelectedTrack;
            Clip clip = SelectedClip;
            int next = arrangement.SelectedSection + 1;

            if (track == null || clip == null || next >= song.SectionCount)
            {
                return;
            }

            CloseOpenClip();
            song.Place(track, next, clip);
            Select(track, next);
        }

        internal void AddTrack(TrackKind kind)
        {
            if (song.Tracks.Count >= Song.MaximumTracks)
            {
                return;
            }

            Track track = song.AddTrack(kind);
            song.Place(track, arrangement.SelectedSection, song.CreateClip(track));
            Select(track, arrangement.SelectedSection);
        }

        internal void ToggleMute([NotNull] Track track)
        {
            track.IsMuted = !track.IsMuted;
            Arrangement.InvalidateVisual();
        }

        /// <summary>
        /// The menu for a cell: repeat or copy its clip into the next section, reuse another of the track's
        /// clips, or clear it.
        /// </summary>
        private void ShowCellMenu([NotNull] CellEventArgs cell)
        {
            Clip clip = cell.Track.ClipAt(cell.Section);
            bool hasNext = cell.Section + 1 < song.SectionCount;
            string command = OperatingSystem.IsMacOS() ? "⌘D" : "Ctrl+D";
            var menu = new ContextMenu();

            if (clip == null)
            {
                menu.Items.Add(MenuItem("New clip", () => CreateClipAt(cell.Track, cell.Section)));
            }
            else
            {
                menu.Items.Add(MenuItem($"Repeat in next section ({command})", RepeatSelectedClip, hasNext,
                    "Plays the same clip again; editing either changes both"));
                menu.Items.Add(MenuItem("Copy to next section", () =>
                {
                    CloseOpenClip();
                    song.Place(cell.Track, cell.Section + 1, song.CopyClip(clip));
                    Select(cell.Track, cell.Section + 1);
                }, hasNext, "A new clip with the same notes, to change separately"));
            }

            List<Clip> others = cell.Track.Clips.Where(other => other != clip).ToList();

            if (others.Count > 0)
            {
                var use = new MenuItem { Header = "Use clip" };

                foreach (Clip other in others)
                {
                    use.Items.Add(MenuItem($"Clip {other.Name}", () =>
                    {
                        CloseOpenClip();
                        song.Place(cell.Track, cell.Section, other);
                        Select(cell.Track, cell.Section);
                    }));
                }

                menu.Items.Add(use);
            }

            if (clip != null)
            {
                menu.Items.Add(new Separator());
                menu.Items.Add(MenuItem("Remove from section", () =>
                {
                    CloseOpenClip();
                    song.Place(cell.Track, cell.Section, null);
                    Select(cell.Track, cell.Section);
                }));
            }

            menu.Open(Arrangement);
        }

        [NotNull]
        private static MenuItem MenuItem([NotNull] string header, [NotNull] Action action, bool enabled = true, [CanBeNull] string tip = null)
        {
            var item = new MenuItem { Header = header, IsEnabled = enabled };
            item.Click += (sender, args) => action();

            if (tip != null)
            {
                ToolTip.SetTip(item, tip);
            }

            return item;
        }

        /// <summary>
        /// Makes sure the selected cell has a clip of the given kind, selecting the first such track if needed.
        /// </summary>
        private void EnsureClip(TrackKind kind)
        {
            Track track = arrangement.SelectedTrack?.Kind == kind ? arrangement.SelectedTrack : song.Tracks.FirstOrDefault(t => t.Kind == kind);

            if (track == null)
            {
                return;
            }

            if (track != arrangement.SelectedTrack)
            {
                Select(track, arrangement.SelectedSection);
            }

            if (SelectedClip == null)
            {
                CreateClipAt(track, arrangement.SelectedSection);
            }
        }

        private void CloseOpenClip()
        {
            openNoteClip?.Close();
            openNoteClip = null;

            if (openDrumPattern != null)
            {
                openDrumPattern.Changed -= OnOpenDrumPatternChanged;
                openDrumPattern = null;
            }
        }

        private void OnOpenDrumPatternChanged(object sender, EventArgs e)
        {
            DrumEmptyHint.IsVisible = openDrumPattern?.IsEmpty ?? false;
            song.NotifyClipEdited();
        }

        /// <summary>
        /// Asks before replacing work, in a small prompt under the button that asked for it.
        /// </summary>
        private void ConfirmReplace([NotNull] string question, [NotNull] Avalonia.Controls.Control anchor, [NotNull] Action onReplace)
        {
            var cancel = new Button { Content = "Cancel", Focusable = false };
            var replace = new Button { Content = "Replace", Focusable = false };
            cancel.Classes.Add("quiet");
            replace.Classes.Add("quiet");
            replace.Classes.Add("accent");

            var flyout = new Flyout
            {
                Placement = PlacementMode.BottomEdgeAlignedRight,
                Content = new StackPanel
                {
                    Spacing = 12,
                    MaxWidth = 260,
                    Children =
                    {
                        new TextBlock { Text = question, TextWrapping = TextWrapping.Wrap, FontWeight = FontWeight.SemiBold },
                        new TextBlock { Text = "This can't be undone.", Classes = { "label" } },
                        new StackPanel { Orientation = Orientation.Horizontal, Spacing = 8, HorizontalAlignment = HorizontalAlignment.Right, Children = { cancel, replace } }
                    }
                }
            };

            cancel.Click += (sender, args) => flyout.Hide();
            replace.Click += (sender, args) =>
            {
                flyout.Hide();
                replace.IsEnabled = false;
                onReplace();
            };
            flyout.Closed += (sender, args) => ReplaceConfirmButton = null;

            ReplaceConfirmButton = replace;
            flyout.ShowAt(anchor);
        }

        /// <summary>
        /// Replaces the notes with the tune, sets its tempo and sound, and plays it.
        /// </summary>
        internal void LoadTune([NotNull] Tune tune)
        {
            if (viewModel.StopSequencer.CanExecute(null))
            {
                viewModel.StopSequencer.Execute(null);
            }

            Editor.ReplaceNotes(tune.ToTones(settings.TimeSignature, settings.TicksPerQuarterNote));
            viewModel.BeatsPerMinute = tune.BeatsPerMinute;
            synthViewModel.Waveform = tune.Waveform;
            ScrollNotesIntoView();
            viewModel.PlaySequencer.Execute(null);
            Log.Info($"Loaded tune {tune.Name}");
        }

        private void BuildTunesMenu()
        {
            var menu = new MenuFlyout { Placement = PlacementMode.BottomEdgeAlignedRight };

            foreach (Tune tune in TuneLibrary.All)
            {
                var item = new MenuItem
                {
                    Header = new StackPanel
                    {
                        Spacing = 1,
                        Children =
                        {
                            new TextBlock { Text = tune.Name },
                            new TextBlock { Text = tune.Credit, FontSize = 11, Foreground = (IBrush) this.FindResource("TextTertiaryBrush") }
                        }
                    }
                };
                item.Click += (sender, args) => RequestTune(tune);
                menu.Items.Add(item);
            }

            TunesButton.Flyout = menu;
        }

        private void BuildLoopsMenu()
        {
            var menu = new MenuFlyout { Placement = PlacementMode.BottomEdgeAlignedRight };

            foreach (DrumLoop loop in DrumLoopLibrary.All)
            {
                var item = new MenuItem
                {
                    Header = new StackPanel
                    {
                        Spacing = 1,
                        Children =
                        {
                            new TextBlock { Text = loop.Name },
                            new TextBlock { Text = $"{loop.Style} · {loop.BeatsPerMinute} BPM", FontSize = 11, Foreground = (IBrush) this.FindResource("TextTertiaryBrush") }
                        }
                    }
                };
                item.Click += (sender, args) => RequestLoop(loop);
                menu.Items.Add(item);
            }

            LoopsButton.Flyout = menu;
        }

        /// <summary>
        /// Scrolls the grid so the notes are in view, if any are off screen.
        /// </summary>
        private void ScrollNotesIntoView()
        {
            List<int> noteNumbers = notes.AllNotes.Select(note => protocol.ProtocolNoteNumber(note.Pitch)).ToList();

            if (noteNumbers.Count == 0 || Editor.Bounds.Height <= 0)
            {
                return;
            }

            int lowestOnKeyboard = protocol.ProtocolNoteNumber(settings.LowestPitch);
            double lane = Editor.Bounds.Height / settings.TotalNotes;
            double top = (settings.TotalNotes - 1 - (noteNumbers.Max() - lowestOnKeyboard)) * lane;
            double bottom = (settings.TotalNotes - (noteNumbers.Min() - lowestOnKeyboard)) * lane;
            double viewport = EditorScrollViewer.Viewport.Height;
            double offset = EditorScrollViewer.Offset.Y;

            if (top >= offset && bottom <= offset + viewport)
            {
                return;
            }

            double centred = (top + bottom) / 2 - viewport / 2;
            double maximum = Math.Max(0, EditorScrollViewer.Extent.Height - viewport);
            EditorScrollViewer.Offset = new Vector(0, Math.Clamp(centred, 0, maximum));
        }

        private void OnViewModelPropertyChanged(object sender, [NotNull] PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(SequencerViewModel.CurrentPosition) || e.PropertyName == nameof(SequencerViewModel.SequencerPlayState))
            {
                UpdatePlayingKeys();
            }

            if (e.PropertyName == nameof(SequencerViewModel.SongTick) || e.PropertyName == nameof(SequencerViewModel.SequencerPlayState))
            {
                // The arrangement's playhead shows while playing or paused, not when stopped.
                Arrangement.PlayheadTick = viewModel.SequencerPlayState == PlayState.Stop ? -1 : viewModel.SongTick;
            }

            if (e.PropertyName == nameof(SequencerViewModel.NoteAction) || e.PropertyName == nameof(SequencerViewModel.IsDrumTrack))
            {
                UpdateEmptyHint();
            }
        }

        /// <summary>
        /// The "click to draw" hint shows only on an empty grid with the Draw tool.
        /// </summary>
        private void UpdateEmptyHint()
        {
            EmptyHint.IsVisible = viewModel.IsSynthTrack && viewModel.IsDrawTool && SelectedClip != null && !notes.AllNotes.Any();
        }

        /// <summary>
        /// Lights the keyboard keys for the notes sounding at the playback position.
        /// </summary>
        private void UpdatePlayingKeys()
        {
            if (viewModel.SequencerPlayState != PlayState.Play)
            {
                Keyboard.SetActivePitches(Enumerable.Empty<Pitch>());
                DrumEditor.CurrentStep = -1;
                return;
            }

            Domain.IPosition position = viewModel.CurrentPosition;
            DrumEditor.CurrentStep = position.TotalTicks(settings.TimeSignature, settings.TicksPerQuarterNote) / (settings.TicksPerQuarterNote / 4);
            Keyboard.SetActivePitches(notes.AllNotes
                .Where(note => note.StartPosition.IsLessThanOrEqual(position) && note.EndPosition.IsGreaterThan(position))
                .Select(note => note.Pitch));
        }

        [CanBeNull]
        private static NoteAction ToolForKey(Key key)
        {
            switch (key)
            {
                case Key.V: return NoteAction.Select;
                case Key.D: return NoteAction.Create;
                case Key.E: return NoteAction.Delete;
                default: return null;
            }
        }

        private void OnEditorPointerPressed(object sender, [NotNull] PointerPressedEventArgs e)
        {
            keyboardState.CurrentModifiers = e.KeyModifiers;
            PointerUpdateKind kind = e.GetCurrentPoint(Editor).Properties.PointerUpdateKind;

            // On macOS, Ctrl-click is the conventional secondary click.
            bool isMacControlClick = OperatingSystem.IsMacOS() && kind == PointerUpdateKind.LeftButtonPressed &&
                                     e.KeyModifiers.HasFlag(KeyModifiers.Control);

            if (kind == PointerUpdateKind.RightButtonPressed || isMacControlClick)
            {
                contextMenuButton = kind == PointerUpdateKind.RightButtonPressed ? MouseButton.Right : MouseButton.Left;
                mouseOperator.IsContextButtonPressed = true;
                NoteActionMenu.BuildPopup(new MousePoint(e.GetPosition(NoteActionMenu)));
            }
            else if (kind == PointerUpdateKind.LeftButtonPressed)
            {
                mouseOperator.IsLeftButtonPressed = true;
                Editor.HandleLeftMouseDown(new MousePoint(e.GetPosition(Editor)));
            }

            e.Pointer.Capture(Editor);
            Focus();
            e.Handled = true;
        }

        private void OnEditorPointerMoved(object sender, [NotNull] PointerEventArgs e)
        {
            keyboardState.CurrentModifiers = e.KeyModifiers;

            if (mouseOperator.CanModifyContextMenu)
            {
                NoteActionMenu.SetCursorPosition(new MousePoint(e.GetPosition(NoteActionMenu)));
            }
            else
            {
                Editor.HandleMouseMovement(new MousePoint(e.GetPosition(Editor)));
            }

            e.Handled = true;
        }

        private void OnEditorPointerReleased(object sender, [NotNull] PointerReleasedEventArgs e)
        {
            keyboardState.CurrentModifiers = e.KeyModifiers;

            if (contextMenuButton == e.InitialPressMouseButton)
            {
                NoteActionMenu.ClosePopup();
                contextMenuButton = null;
                mouseOperator.IsContextButtonPressed = false;
            }
            else if (e.InitialPressMouseButton == MouseButton.Left)
            {
                Editor.DragSelectionBox.CloseSelectionBox();
                mouseOperator.IsLeftButtonPressed = false;
            }

            e.Pointer.Capture(null);
            e.Handled = true;
        }

        private void ResetPointerState()
        {
            NoteActionMenu.ClosePopup();
            Editor.DragSelectionBox.CloseSelectionBox();
            contextMenuButton = null;
            mouseOperator.IsContextButtonPressed = false;
            mouseOperator.IsLeftButtonPressed = false;
        }

        private void OnKeyDown(object sender, [NotNull] KeyEventArgs e)
        {
            keyboardState.CurrentModifiers = e.KeyModifiers;

            if (e.Source is TextBox)
            {
                // Typing in a text field (such as the tempo box): leave the keys to it.
                if (e.Key == Key.Enter || e.Key == Key.Escape)
                {
                    // Commit or abandon the edit and hand the keyboard back to the editor.
                    Focus();
                    e.Handled = true;
                }

                return;
            }

            if (e.Key == Key.D && e.KeyModifiers == keyboardState.CommandModifier)
            {
                RepeatSelectedClip();
                e.Handled = true;
            }
            else if (e.KeyModifiers == KeyModifiers.None && e.Key == Key.Space)
            {
                viewModel.ToggleSequencer.Execute(null);
                e.Handled = true;
            }
            else if (e.KeyModifiers == KeyModifiers.None && viewModel.IsSynthTrack && ToolForKey(e.Key) is NoteAction tool)
            {
                viewModel.NoteAction = tool;
                e.Handled = true;
            }
            else if (e.KeyModifiers == KeyModifiers.None && e.Key == Key.Enter)
            {
                if (viewModel.StopSequencer.CanExecute(null))
                {
                    viewModel.StopSequencer.Execute(null);
                }

                e.Handled = true;
            }
            else if (viewModel.IsSynthTrack)
            {
                // Note shortcuts only on the synth track: on the drum track the notes aren't on screen.
                e.Handled = Editor.HandleKeyPress(e.Key, e.KeyModifiers);
            }
        }
    }
}
