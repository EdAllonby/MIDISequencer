using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using JetBrains.Annotations;
using Sequencer.Domain;
using Sequencer.Midi;
using Sequencer.Shared;
using Sequencer.View.Command;
using Sequencer.View.Command.MousePointCommand;
using Sequencer.View.Command.NotesCommand;
using Sequencer.View.Drawing;
using Sequencer.View.Input;
using Sequencer.ViewModel;

namespace Sequencer.View.Control
{
    public partial class SequencerControl
    {
        /// <summary>
        /// The NoteAction Dependency Property.
        /// </summary>
        [NotNull] public static readonly DependencyProperty NoteActionProperty =
            DependencyProperty.Register(nameof(NoteAction), typeof(NoteAction), typeof(SequencerControl),
                new FrameworkPropertyMetadata(null));

        [NotNull] public static readonly DependencyProperty SelectedNotesProperty =
            DependencyProperty.Register(nameof(SelectedNotes), typeof(IEnumerable<Tone>), typeof(SequencerControl),
                new FrameworkPropertyMetadata(null));

        [NotNull] public static readonly DependencyProperty CurrentPositionProperty =
            DependencyProperty.Register(nameof(CurrentPosition), typeof(IPosition), typeof(SequencerControl),
                new FrameworkPropertyMetadata(OnCurrentPositionChanged));

        [NotNull] private readonly IKeyboardStateProcessor keyboardStateProcessor = new KeyboardStateProcessor();
        [NotNull] private readonly SequencerKeyPressCommandHandler keyPressCommandHandler;
        [NotNull] private readonly IMouseOperator mouseOperator = new MouseOperator(new MouseStateProcessor());
        [NotNull] private readonly MousePointNoteCommandFactory mousePointNoteCommandFactory;
        [NotNull] private readonly ISequencerNotes notes;
        [NotNull] private readonly IPitchAndPositionCalculator pitchAndPositionCalculator;
        [NotNull] private readonly PositionIndicatorDrawer positionIndicatorDrawer;
        [NotNull] private readonly UpdateNoteStateCommand selectNoteCommand;
        [NotNull] private readonly ISequencerDimensionsCalculator sequencerDimensionsCalculator;
        [NotNull] private readonly SequencerDrawer sequencerDrawer;
        [NotNull] private readonly SequencerSettings sequencerSettings = new SequencerSettings();

        [NotNull] private readonly UpdateNewlyCreatedNoteCommand updateNewlyCreatedNoteCommand;
        [CanBeNull] private IMousePointNoteCommand moveNoteFromPointCommand;

        public SequencerControl()
        {
            InitializeComponent();

            SizeChanged += SequencerSizeChanged;
            notes = new SequencerNotes(sequencerSettings);
            notes.SelectedNotesChanged += SelectedNotesChanged;

            if (SequencerCanvas == null)
            {
                throw new NullReferenceException($"{nameof(SequencerCanvas)} is null.");
            }

            ISequencerCanvasWrapper sequencerCanvasWrapper = new SequencerCanvasWrapper(SequencerCanvas);
            pitchAndPositionCalculator = new PitchAndPositionCalculator(sequencerSettings.TimeSignature);
            IDigitalAudioProtocol protocol = new MidiProtocol(pitchAndPositionCalculator);

            sequencerDimensionsCalculator = new SequencerDimensionsCalculator(protocol, sequencerCanvasWrapper, sequencerSettings, pitchAndPositionCalculator);
            IVisualNoteFactory visualNoteFactory = new VisualNoteFactory(sequencerSettings, protocol, sequencerDimensionsCalculator, sequencerCanvasWrapper);

            mousePointNoteCommandFactory = new MousePointNoteCommandFactory(visualNoteFactory, mouseOperator, keyboardStateProcessor, notes, sequencerSettings, sequencerDimensionsCalculator);
            updateNewlyCreatedNoteCommand = new UpdateNewlyCreatedNoteCommand(notes, mouseOperator, sequencerSettings.TimeSignature, sequencerDimensionsCalculator);
            selectNoteCommand = new UpdateNoteStateCommand(notes, keyboardStateProcessor, NoteState.Selected);
            sequencerDrawer = new SequencerDrawer(protocol, sequencerCanvasWrapper, notes, sequencerDimensionsCalculator, sequencerSettings);
            positionIndicatorDrawer = new PositionIndicatorDrawer(sequencerSettings, sequencerCanvasWrapper, sequencerDimensionsCalculator);

            keyPressCommandHandler = new SequencerKeyPressCommandHandler(notes, keyboardStateProcessor);
        }

        public NoteAction NoteAction
        {
            get => (NoteAction) GetValue(NoteActionProperty);
            set => SetValue(NoteActionProperty, value);
        }

        [NotNull]
        [ItemNotNull]
        public IEnumerable<Tone> SelectedNotes
        {
            get => (IEnumerable<Tone>) GetValue(SelectedNotesProperty) ?? throw new InvalidOperationException();
            set => SetValue(SelectedNotesProperty, value);
        }

        [NotNull]
        public IPosition CurrentPosition
        {
            get => (IPosition) GetValue(CurrentPositionProperty) ?? throw new InvalidOperationException();
            set => SetValue(CurrentPositionProperty, value);
        }

        public void HandleLeftMouseDown([NotNull] IMousePoint mouseDownPoint)
        {
            if (DragSelectionBox == null)
            {
                throw new NullReferenceException($"{nameof(DragSelectionBox)} is null.");
            }
            if (NoteAction == null)
            {
                throw new NullReferenceException($"{nameof(NoteAction)} is null.");
            }

            if (!sequencerDimensionsCalculator.IsPointInsideNote(notes, mouseDownPoint)
                && NoteAction == NoteAction.Select)
            {
                DragSelectionBox.SetNewOriginMousePosition(mouseDownPoint);
            }

            IMousePointNoteCommand noteCommand = mousePointNoteCommandFactory.FindCommand(NoteAction);

            moveNoteFromPointCommand = GetMovementCommand(mouseDownPoint);

            noteCommand.Execute(mouseDownPoint);
        }

        public void HandleKeyPress(Key keyPressed)
        {
            keyPressCommandHandler.HandleSequencerKeyPressed(keyPressed);
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

        private static void OnCurrentPositionChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var component = (SequencerControl) d;
            PositionIndicatorDrawer indicatorDrawer = component.positionIndicatorDrawer;
            indicatorDrawer.DrawPositionIndicator((IPosition) e.NewValue);
        }

        private void SelectedNotesChanged(object sender, [NotNull] [ItemNotNull] IEnumerable<IVisualNote> e)
        {
            SelectedNotes = e.Select(x => x.Tone);
        }

        [CanBeNull]
        private IMousePointNoteCommand GetMovementCommand([NotNull] IMousePoint mouseDownPoint)
        {
            IVisualNote noteAtEndingPoint = sequencerDimensionsCalculator.NoteAtEndingPoint(notes, mouseDownPoint);
            if (noteAtEndingPoint != null)
            {
                return new UpdateNoteEndPositionFromInitialPointCommand(mouseDownPoint, mouseOperator, notes, sequencerSettings, sequencerDimensionsCalculator);
            }

            IVisualNote noteAtStartingPoint = sequencerDimensionsCalculator.NoteAtStartingPoint(notes, mouseDownPoint);
            if (noteAtStartingPoint != null)
            {
                return new UpdateNoteStartPositionFromInitialPointCommand(mouseDownPoint, mouseOperator, notes, sequencerSettings, sequencerDimensionsCalculator);
            }

            IVisualNote noteUnderMouse = sequencerDimensionsCalculator.FindNoteFromPoint(notes, mouseDownPoint);
            if (noteUnderMouse != null)
            {
                return new MoveNoteFromPointCommand(keyboardStateProcessor, pitchAndPositionCalculator, mouseDownPoint, mouseOperator, notes, sequencerDimensionsCalculator);
            }

            return null;
        }

        private void HandleMouseSelectMovement([NotNull] IMousePoint newMousePoint)
        {
            if (DragSelectionBox == null)
            {
                throw new NullReferenceException($"{nameof(DragSelectionBox)} is null.");
            }

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
            IVisualNote noteAtStartingPoint = sequencerDimensionsCalculator.NoteAtStartingPoint(notes, newMousePoint);
            if (noteAtStartingPoint != null)
            {
                Mouse.OverrideCursor = Cursors.No;
                return;
            }

            IVisualNote noteAtEndingPoint = sequencerDimensionsCalculator.NoteAtEndingPoint(notes, newMousePoint);
            if (noteAtEndingPoint != null)
            {
                Mouse.OverrideCursor = Cursors.ArrowCD;
                return;
            }

            IVisualNote noteUnderMouse = sequencerDimensionsCalculator.FindNoteFromPoint(notes, newMousePoint);

            if (noteUnderMouse != null)
            {
                Mouse.OverrideCursor = Cursors.Hand;
                return;
            }

            Mouse.OverrideCursor = null;
        }

        private void SequencerSizeChanged(object sender, RoutedEventArgs e)
        {
            sequencerDrawer.RedrawEditor();
            positionIndicatorDrawer.RedrawEditor();
        }
    }
}