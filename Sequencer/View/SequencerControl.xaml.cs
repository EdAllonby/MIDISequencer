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
        private readonly UpdateNoteEndPositionFromPointCommand updateNoteEndPositionFromPointCommand;
        private MoveNoteFromPointCommand command;

        public SequencerControl()
        {
            InitializeComponent();

            SizeChanged += SequencerSizeChanged;
            notes = new SequencerNotes();
            sequencerDimensionsCalculator = new SequencerDimensionsCalculator(SequencerCanvas, sequencerSettings);
            mousePointNoteCommandFactory = new MousePointNoteCommandFactory(SequencerCanvas, notes, sequencerSettings, sequencerDimensionsCalculator);
            updateNoteEndPositionFromPointCommand = new UpdateNoteEndPositionFromPointCommand(notes, sequencerSettings, sequencerDimensionsCalculator);
            selectNoteCommand = new UpdateNoteStateCommand(notes, NoteState.Selected);
            sequencerDrawer = new SequencerDrawer(SequencerCanvas, notes, sequencerDimensionsCalculator, sequencerSettings);
            keyPressCommandHandler = new SequencerKeyPressCommandHandler(notes);
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
            command = new MoveNoteFromPointCommand(mouseDownPoint, notes, sequencerSettings, sequencerDimensionsCalculator);
            noteCommand.Execute(mouseDownPoint);
        }

        public void HandleKeyPress(Key keyPressed)
        {
            keyPressCommandHandler.HandleSequencerKeyPressed(keyPressed);
        }

        public void HandleMouseMovement(Point newMousePoint)
        {
            if (NoteAction == NoteAction.Create)
            {
                updateNoteEndPositionFromPointCommand.Execute(newMousePoint);
            }
            if (NoteAction == NoteAction.Select)
            {
                VisualNote noteUnderMouse = sequencerDimensionsCalculator.FindNoteFromPoint(notes, newMousePoint);

                DragSelectionBox.UpdateDragSelectionBox(newMousePoint);

                if (!DragSelectionBox.IsDragging)
                {
                    Mouse.OverrideCursor = noteUnderMouse != null ? Cursors.Hand : null;
                    command?.Execute(newMousePoint);
                }
                else
                {
                    IEnumerable<VisualNote> containedNotes = DragSelectionBox.FindMatches(notes.All);
                    SelectedNotes = notes.SelectedNotes.Select(x => x.Tone);
                    selectNoteCommand.Execute(containedNotes);
                }
            }
        }

        private void SequencerSizeChanged(object sender, RoutedEventArgs e)
        {
            sequencerDrawer.RedrawEditor();
        }
    }
}