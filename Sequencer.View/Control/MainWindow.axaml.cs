using System;
using System.ComponentModel;
using System.Linq;
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
            [NotNull] SynthViewModel synthViewModel, [NotNull] INotePreviewer notePreviewer)
        {
            InitializeComponent();

            this.viewModel = viewModel;
            this.notes = notes;
            this.keyboardState = keyboardState;
            this.mouseOperator = mouseOperator;
            DataContext = viewModel;
            SynthPanel.DataContext = synthViewModel;

            Header.Initialise(pitchAndPositionCalculator, protocol, settings);
            Editor.Initialise(notes, pitchAndPositionCalculator, protocol, settings, keyboardState, mouseOperator, keyboardState.CommandModifier, notePreviewer);

            Keyboard.Initialise(protocol, settings, notePreviewer);
            viewModel.PropertyChanged += OnViewModelPropertyChanged;

            Editor.PointerPressed += OnEditorPointerPressed;
            Editor.PointerMoved += OnEditorPointerMoved;
            Editor.PointerReleased += OnEditorPointerReleased;
            Editor.PointerCaptureLost += (sender, args) => ResetPointerState();

            // Tunnel, so shortcuts reach the editor before controls such as the scroll viewer consume them.
            AddHandler(KeyDownEvent, OnKeyDown, RoutingStrategies.Tunnel);
            AddHandler(KeyUpEvent, (sender, args) => keyboardState.CurrentModifiers = args.KeyModifiers, RoutingStrategies.Tunnel);

            Log.Info("Main window loaded");
        }

        private void OnViewModelPropertyChanged(object sender, [NotNull] PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(SequencerViewModel.CurrentPosition) || e.PropertyName == nameof(SequencerViewModel.SequencerPlayState))
            {
                UpdatePlayingKeys();
            }
        }

        /// <summary>
        /// Lights the keyboard keys for the notes sounding at the playback position.
        /// </summary>
        private void UpdatePlayingKeys()
        {
            if (viewModel.SequencerPlayState != PlayState.Play)
            {
                Keyboard.SetActivePitches(Enumerable.Empty<Pitch>());
                return;
            }

            Domain.IPosition position = viewModel.CurrentPosition;
            Keyboard.SetActivePitches(notes.AllNotes
                .Where(note => note.StartPosition.IsLessThanOrEqual(position) && note.EndPosition.IsGreaterThan(position))
                .Select(note => note.Pitch));
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

            if (e.KeyModifiers == KeyModifiers.None && e.Key == Key.Space)
            {
                viewModel.ToggleSequencer.Execute(null);
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
            else
            {
                e.Handled = Editor.HandleKeyPress(e.Key, e.KeyModifiers);
            }
        }
    }
}
