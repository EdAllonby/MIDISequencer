using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Media;
using JetBrains.Annotations;
using Sequencer.Domain;
using Sequencer.Domain.Settings;
using Sequencer.Midi;
using Sequencer.View.Command;
using Sequencer.View.Command.MousePointCommand;
using Sequencer.View.Command.NotesCommand;
using Sequencer.View.Drawing;
using Sequencer.ViewModel;
using Sequencer.Visual;
using Sequencer.Visual.Input;

namespace Sequencer.View.Control
{
    /// <summary>
    /// The piano-roll editor: note lanes, notes, the playback position and drag selection.
    /// </summary>
    public sealed class SequencerControl : UserControl
    {
        public static readonly StyledProperty<NoteAction> NoteActionProperty =
            AvaloniaProperty.Register<SequencerControl, NoteAction>(nameof(NoteAction));

        public static readonly StyledProperty<IEnumerable<Tone>> SelectedNotesProperty =
            AvaloniaProperty.Register<SequencerControl, IEnumerable<Tone>>(nameof(SelectedNotes), Enumerable.Empty<Tone>(),
                defaultBindingMode: BindingMode.TwoWay);

        public static readonly StyledProperty<IPosition> CurrentPositionProperty =
            AvaloniaProperty.Register<SequencerControl, IPosition>(nameof(CurrentPosition));

        [NotNull] private static readonly Cursor ResizeCursor = new Cursor(StandardCursorType.SizeWestEast);
        [NotNull] private static readonly Cursor MoveCursor = new Cursor(StandardCursorType.Hand);

        [NotNull] private readonly Canvas sequencerCanvas = new Canvas { Background = Brushes.Transparent };

        // Set by Initialise. The control is created by XAML, which needs a parameterless constructor.
        private IKeyboardStateProcessor keyboardStateProcessor;
        private SequencerKeyPressCommandHandler keyPressCommandHandler;
        private IMouseOperator mouseOperator;
        private MousePointNoteCommandFactory mousePointNoteCommandFactory;
        private ISequencerNotes notes;
        private IPitchAndPositionCalculator pitchAndPositionCalculator;
        private PositionIndicatorDrawer positionIndicatorDrawer;
        private UpdateNoteStateCommand selectNoteCommand;
        private ISequencerDimensionsCalculator sequencerDimensionsCalculator;
        private SequencerDrawer sequencerDrawer;
        private SequencerSettings sequencerSettings;
        private UpdateNewlyCreatedNoteCommand updateNewlyCreatedNoteCommand;
        [CanBeNull] private IMousePointNoteCommand moveNoteFromPointCommand;

        public SequencerControl()
        {
            var grid = new Grid();
            grid.Children.Add(sequencerCanvas);
            grid.Children.Add(DragSelectionBox);
            Content = grid;

            SizeChanged += (sender, args) => Redraw();
        }

        [NotNull]
        public DragSelectionBox DragSelectionBox { get; } = new DragSelectionBox();

        public NoteAction NoteAction
        {
            get => GetValue(NoteActionProperty);
            set => SetValue(NoteActionProperty, value);
        }

        public IEnumerable<Tone> SelectedNotes
        {
            get => GetValue(SelectedNotesProperty);
            set => SetValue(SelectedNotesProperty, value);
        }

        public IPosition CurrentPosition
        {
            get => GetValue(CurrentPositionProperty);
            set => SetValue(CurrentPositionProperty, value);
        }

        public void Initialise([NotNull] ISequencerNotes sequencerNotes, [NotNull] IPitchAndPositionCalculator calculator,
            [NotNull] IDigitalAudioProtocol protocol, [NotNull] SequencerSettings settings,
            [NotNull] IKeyboardStateProcessor keyboardState, [NotNull] IMouseOperator mouse, KeyModifiers commandModifier,
            [NotNull] INotePreviewer notePreviewer)
        {
            notes = sequencerNotes;
            pitchAndPositionCalculator = calculator;
            sequencerSettings = settings;
            keyboardStateProcessor = keyboardState;
            mouseOperator = mouse;

            notes.SelectedNotesChanged += OnSelectedNotesChanged;

            ISequencerCanvasWrapper canvasWrapper = new SequencerCanvasWrapper(sequencerCanvas);
            sequencerDimensionsCalculator = new SequencerDimensionsCalculator(protocol, canvasWrapper, sequencerSettings, pitchAndPositionCalculator);
            IVisualNoteFactory visualNoteFactory = new VisualNoteFactory(sequencerSettings, protocol, sequencerDimensionsCalculator, canvasWrapper, notePreviewer);
            INoteStateCommandFactory noteStateCommandFactory = new NoteStateCommandFactory(notes, keyboardStateProcessor);

            mousePointNoteCommandFactory = new MousePointNoteCommandFactory(visualNoteFactory, mouseOperator, keyboardStateProcessor, notes,
                sequencerSettings, sequencerDimensionsCalculator, noteStateCommandFactory, notePreviewer);
            updateNewlyCreatedNoteCommand = new UpdateNewlyCreatedNoteCommand(notes, mouseOperator, sequencerSettings, sequencerDimensionsCalculator);
            selectNoteCommand = new UpdateNoteStateCommand(notes, keyboardStateProcessor, NoteState.Selected);
            sequencerDrawer = new SequencerDrawer(protocol, canvasWrapper, notes, sequencerDimensionsCalculator, sequencerSettings);
            positionIndicatorDrawer = new PositionIndicatorDrawer(sequencerSettings, canvasWrapper, sequencerDimensionsCalculator);
            keyPressCommandHandler = new SequencerKeyPressCommandHandler(sequencerSettings, notes, keyboardStateProcessor, commandModifier);

            Redraw();
        }

        public void HandleLeftMouseDown([NotNull] IMousePoint mouseDownPoint)
        {
            NoteAction noteAction = NoteAction ?? throw new InvalidOperationException($"{nameof(NoteAction)} is not set.");

            if (noteAction == NoteAction.Select && !sequencerDimensionsCalculator.IsPointInsideNote(notes, mouseDownPoint))
            {
                DragSelectionBox.SetNewOriginMousePosition(mouseDownPoint);
            }

            IMousePointNoteCommand noteCommand = mousePointNoteCommandFactory.FindCommand(noteAction);
            moveNoteFromPointCommand = GetMovementCommand(mouseDownPoint);
            noteCommand.Execute(mouseDownPoint);
        }

        /// <summary>
        /// Returns whether the key was a sequencer shortcut.
        /// </summary>
        public bool HandleKeyPress(Key keyPressed, KeyModifiers modifiers)
        {
            return keyPressCommandHandler?.HandleSequencerKeyPressed(keyPressed, modifiers) ?? false;
        }

        public void HandleMouseMovement([NotNull] IMousePoint newMousePoint)
        {
            if (NoteAction == NoteAction.Create)
            {
                updateNewlyCreatedNoteCommand.Execute(newMousePoint);
            }

            if (NoteAction == NoteAction.Select)
            {
                HandleMouseSelectMovement(newMousePoint);
            }
        }

        protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
        {
            base.OnPropertyChanged(change);

            if (change.Property == CurrentPositionProperty && change.NewValue is IPosition newPosition)
            {
                positionIndicatorDrawer?.DrawPositionIndicator(newPosition);
            }
        }

        private void Redraw()
        {
            sequencerDrawer?.RedrawEditor();
            positionIndicatorDrawer?.RedrawEditor();
        }

        private void OnSelectedNotesChanged(object sender, [NotNull] SelectedNotesEventArgs e)
        {
            SelectedNotes = e.SelectedVisualNotes.Select(x => x.Tone).ToList();
        }

        [CanBeNull]
        private IMousePointNoteCommand GetMovementCommand([NotNull] IMousePoint mouseDownPoint)
        {
            IVisualNote note = sequencerDimensionsCalculator.FindNoteFromPoint(notes, mouseDownPoint);

            if (note == null)
            {
                return null;
            }

            switch (sequencerDimensionsCalculator.EdgeAtPoint(note, mouseDownPoint))
            {
                case NoteEdge.End:
                    return new UpdateNoteEndPositionFromInitialPointCommand(mouseDownPoint, mouseOperator, notes, sequencerSettings, sequencerDimensionsCalculator);
                case NoteEdge.Start:
                    return new UpdateNoteStartPositionFromInitialPointCommand(mouseDownPoint, mouseOperator, notes, sequencerSettings, sequencerDimensionsCalculator);
                default:
                    return new MoveNoteFromPointCommand(keyboardStateProcessor, pitchAndPositionCalculator, mouseDownPoint, mouseOperator, notes, sequencerDimensionsCalculator);
            }
        }

        private void HandleMouseSelectMovement([NotNull] IMousePoint newMousePoint)
        {
            DragSelectionBox.UpdateDragSelectionBox(newMousePoint);

            if (DragSelectionBox.IsDragging)
            {
                IEnumerable<IVisualNote> containedNotes = DragSelectionBox.FindMatches(notes.AllNotes);
                selectNoteCommand.Execute(containedNotes);
            }
            else
            {
                SetCursor(newMousePoint);
                moveNoteFromPointCommand?.Execute(newMousePoint);
            }
        }

        private void SetCursor([NotNull] IMousePoint newMousePoint)
        {
            IVisualNote note = sequencerDimensionsCalculator.FindNoteFromPoint(notes, newMousePoint);

            if (note == null)
            {
                Cursor = Cursor.Default;
            }
            else
            {
                Cursor = sequencerDimensionsCalculator.EdgeAtPoint(note, newMousePoint) == NoteEdge.None ? MoveCursor : ResizeCursor;
            }
        }
    }
}
