using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using Sequencer.Command;
using Sequencer.Command.MousePointCommand;
using Sequencer.Command.NotesCommand;
using Sequencer.Domain;
using Sequencer.Drawing;
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
        private readonly SequencerNotes notes;
        private readonly UpdateNoteStateCommand selectNoteCommand;
        private readonly SequencerDimensionsCalculator sequencerDimensionsCalculator;
        private readonly SequencerDrawer sequencerDrawer;
        private readonly SequencerSettings sequencerSettings = new SequencerSettings();
        private readonly UpdateNewlyCreatedNoteCommand updateNewlyCreatedNoteCommand;
        private IMousePointNoteCommand moveNoteFromPointCommand;

        public SequencerControl()
        {
            InitializeComponent();

            SizeChanged += SequencerSizeChanged;
            notes = new SequencerNotes(sequencerSettings);
            notes.SelectedNotesChanged += SelectedNotesChanged;


            sequencerDimensionsCalculator = new SequencerDimensionsCalculator(SequencerCanvas, sequencerSettings);
            mousePointNoteCommandFactory = new MousePointNoteCommandFactory(SequencerCanvas, notes, sequencerSettings, sequencerDimensionsCalculator);
            updateNewlyCreatedNoteCommand = new UpdateNewlyCreatedNoteCommand(notes, sequencerSettings, sequencerDimensionsCalculator);
            selectNoteCommand = new UpdateNoteStateCommand(notes, NoteState.Selected);
            sequencerDrawer = new SequencerDrawer(SequencerCanvas, notes, sequencerDimensionsCalculator, sequencerSettings);
            keyPressCommandHandler = new SequencerKeyPressCommandHandler(notes);
        }

        private void SelectedNotesChanged(object sender, IEnumerable<VisualNote> e)
        {
            SelectedNotes = e.Select(x => x.Tone);
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

        public void HandleLeftMouseDown(Point mouseDownPoint)
        {
            if (!sequencerDimensionsCalculator.IsPointInsideNote(notes, mouseDownPoint)
                && (NoteAction == NoteAction.Select))
            {
                DragSelectionBox.SetNewOriginMousePosition(mouseDownPoint);
            }

            MousePointNoteCommand noteCommand = mousePointNoteCommandFactory.FindCommand(NoteAction);

            moveNoteFromPointCommand = GetMovementCommand(mouseDownPoint);

            noteCommand.Execute(mouseDownPoint);
        }

        private IMousePointNoteCommand GetMovementCommand(Point mouseDownPoint)
        {
            VisualNote noteAtEndingPoint = sequencerDimensionsCalculator.NoteAtEndingPoint(notes, mouseDownPoint);
            if (noteAtEndingPoint != null)
            {
                return new UpdateNoteEndPositionFromInitialPointCommand(mouseDownPoint, notes, sequencerSettings, sequencerDimensionsCalculator);
            }

            VisualNote noteAtStartingPoint = sequencerDimensionsCalculator.NoteAtStartingPoint(notes, mouseDownPoint);
            if (noteAtStartingPoint != null)
            {
                return new UpdateNoteStartPositionFromInitialPointCommand(mouseDownPoint, notes, sequencerSettings, sequencerDimensionsCalculator);
            }
            
            VisualNote noteUnderMouse = sequencerDimensionsCalculator.FindNoteFromPoint(notes, mouseDownPoint);
            if (noteUnderMouse != null)
            {
                return new MoveNoteFromPointCommand(mouseDownPoint, notes, sequencerSettings, sequencerDimensionsCalculator);
            }

            return null;
        }

        public void HandleKeyPress(Key keyPressed)
        {
            keyPressCommandHandler.HandleSequencerKeyPressed(keyPressed);
        }

        public void HandleMouseMovement(Point newMousePoint)
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

        private void HandleMouseSelectMovement(Point newMousePoint)
        {
            DragSelectionBox.UpdateDragSelectionBox(newMousePoint);

            if (DragSelectionBox.IsDragging)
            {
                IEnumerable<VisualNote> containedNotes = DragSelectionBox.FindMatches(notes.AllNotes);
                selectNoteCommand.Execute(containedNotes);
            }
            else
            {
                SetCursor(newMousePoint);

                moveNoteFromPointCommand?.Execute(newMousePoint);
            }
        }

        private void SetCursor(Point newMousePoint)
        {
            var noteAtStartingPoint = sequencerDimensionsCalculator.NoteAtStartingPoint(notes, newMousePoint);
            if (noteAtStartingPoint != null)
            {
                Mouse.OverrideCursor = Cursors.No;
                return;
            }

            var noteAtEndingPoint = sequencerDimensionsCalculator.NoteAtEndingPoint(notes, newMousePoint);
            if (noteAtEndingPoint != null)
            {
                Mouse.OverrideCursor = Cursors.ArrowCD;
                return;

            }

            VisualNote noteUnderMouse = sequencerDimensionsCalculator.FindNoteFromPoint(notes, newMousePoint);

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