using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using Sequencer.Command;
using Sequencer.Command.MousePointCommand;
using Sequencer.Command.NotesCommand;
using Sequencer.Domain;
using Sequencer.Drawing;
using Sequencer.Input;
using Sequencer.ViewModel;

namespace Sequencer.View
{
    public partial class SequencerControl
    {
        /// <summary>
        /// The NoteAction Dependency Property.
        /// </summary>
        public static readonly DependencyProperty NoteActionProperty =
            DependencyProperty.Register(nameof(NoteAction), typeof(NoteAction), typeof(SequencerControl),
                new FrameworkPropertyMetadata(null));

        public static readonly DependencyProperty SelectedNotesProperty =
            DependencyProperty.Register(nameof(SelectedNotes), typeof(IEnumerable<Tone>), typeof(SequencerControl),
                new FrameworkPropertyMetadata(null));

        private readonly SequencerKeyPressCommandHandler keyPressCommandHandler;
        private readonly MousePointNoteCommandFactory mousePointNoteCommandFactory;
        private readonly ISequencerNotes notes;
        private readonly UpdateNoteStateCommand selectNoteCommand;
        private readonly SequencerDimensionsCalculator sequencerDimensionsCalculator;
        private readonly SequencerDrawer sequencerDrawer;
        private readonly SequencerSettings sequencerSettings = new SequencerSettings();
        private readonly UpdateNewlyCreatedNoteCommand updateNewlyCreatedNoteCommand;
        private readonly IMouseOperator mouseOperator = new MouseOperator(new MouseStateProcessor());

        private IMousePointNoteCommand moveNoteFromPointCommand;
        
        public SequencerControl()
        {
            InitializeComponent();

            SizeChanged += SequencerSizeChanged;
            notes = new SequencerNotes(sequencerSettings);
            notes.SelectedNotesChanged += SelectedNotesChanged;

            var keyboardStateProcessor = new KeyboardStateProcessor();

            ISequencerCanvasWrapper sequencerCanvasWrapper = new SequencerCanvasWrapper(SequencerCanvas);
            sequencerDimensionsCalculator = new SequencerDimensionsCalculator(SequencerCanvas, sequencerSettings);

            IVisualNoteFactory visualNoteFactory = new VisualNoteFactory(sequencerSettings,sequencerDimensionsCalculator, sequencerCanvasWrapper);

            mousePointNoteCommandFactory = new MousePointNoteCommandFactory(visualNoteFactory, mouseOperator, keyboardStateProcessor, notes, sequencerSettings, sequencerDimensionsCalculator);
            updateNewlyCreatedNoteCommand = new UpdateNewlyCreatedNoteCommand(notes, mouseOperator, sequencerSettings, sequencerDimensionsCalculator);
            selectNoteCommand = new UpdateNoteStateCommand(notes, keyboardStateProcessor, NoteState.Selected);
            sequencerDrawer = new SequencerDrawer(SequencerCanvas, notes, sequencerDimensionsCalculator, sequencerSettings);
            keyPressCommandHandler = new SequencerKeyPressCommandHandler(notes, keyboardStateProcessor);
        }

        public NoteAction NoteAction
        {
            get { return (NoteAction) GetValue(NoteActionProperty); }
            set { SetValue(NoteActionProperty, value); }
        }

        public IEnumerable<Tone> SelectedNotes
        {
            get { return (IEnumerable<Tone>) GetValue(SelectedNotesProperty); }
            set { SetValue(SelectedNotesProperty, value); }
        }

        public void AddChild(UIElement child)
        {
            SequencerCanvas.Children.Add(child);
        }

        public void RemoveChild(UIElement child)
        {
            SequencerCanvas.Children.Remove(child);
        }

        public void HandleLeftMouseDown(IMousePoint mouseDownPoint)
        {
            if (!sequencerDimensionsCalculator.IsPointInsideNote(notes, mouseDownPoint)
                && NoteAction == NoteAction.Select)
            {
                DragSelectionBox.SetNewOriginMousePosition(mouseDownPoint);
            }

            MousePointNoteCommand noteCommand = mousePointNoteCommandFactory.FindCommand(NoteAction);

            moveNoteFromPointCommand = GetMovementCommand(mouseDownPoint);

            noteCommand.Execute(mouseDownPoint);
        }

        public void HandleKeyPress(Key keyPressed)
        {
            keyPressCommandHandler.HandleSequencerKeyPressed(keyPressed);
        }

        public void HandleMouseMovement(IMousePoint newMousePoint)
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

        private void SelectedNotesChanged(object sender, IEnumerable<IVisualNote> e)
        {
            SelectedNotes = e.Select(x => x.Tone);
        }

        private IMousePointNoteCommand GetMovementCommand(IMousePoint mouseDownPoint)
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
                return new MoveNoteFromPointCommand(mouseDownPoint, mouseOperator, notes, sequencerSettings, sequencerDimensionsCalculator);
            }

            return null;
        }

        private void HandleMouseSelectMovement(IMousePoint newMousePoint)
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

        private void SetCursor(IMousePoint newMousePoint)
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
        }
    }
}