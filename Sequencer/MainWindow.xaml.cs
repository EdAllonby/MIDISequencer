using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;
using log4net;
using Sequencer.Command;
using Sequencer.Command.MousePointCommand;
using Sequencer.Domain;
using Sequencer.Drawing;
using Sequencer.ViewModel;

namespace Sequencer
{
    public partial class MainWindow
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(MainWindow));

        private readonly MousePointNoteCommandFactory mousePointNoteCommandFactory;

        private readonly List<VisualNote> notes = new List<VisualNote>();
        private readonly SequencerKeyPressHandler keyPressHandler;
        private readonly UpdateNoteStateCommand selectNoteCommand;
        private readonly SequencerDimensionsCalculator sequencerDimensionsCalculator;
        private readonly SequencerDrawer sequencerDrawer;
        private readonly SequencerSettings sequencerSettings = new SequencerSettings();
        private readonly UpdateNoteEndPositionFromPointCommand updateNoteEndPositionFromPointCommand;

        private MoveNoteFromPointCommand command;

        public MainWindow()
        {
            InitializeComponent();
            SizeChanged += WindowChanged;
            sequencerDimensionsCalculator = new SequencerDimensionsCalculator(SequencerCanvas, sequencerSettings);
            mousePointNoteCommandFactory = new MousePointNoteCommandFactory(SequencerCanvas, notes, sequencerSettings, sequencerDimensionsCalculator);
            updateNoteEndPositionFromPointCommand = new UpdateNoteEndPositionFromPointCommand(notes, sequencerSettings, sequencerDimensionsCalculator);
            selectNoteCommand = new UpdateNoteStateCommand(notes, NoteState.Selected);
            sequencerDrawer = new SequencerDrawer(SequencerCanvas, notes, sequencerDimensionsCalculator, sequencerSettings);
            keyPressHandler = new SequencerKeyPressHandler(notes);

            Log.Info("Main Window loaded");
        }

        private SequencerViewModel ViewModel => (SequencerViewModel) DataContext;

        private void WindowChanged(object sender, RoutedEventArgs e)
        {
            sequencerDrawer.RedrawEditor();
        }

        private void SequencerMouseDown(object sender, MouseButtonEventArgs e)
        {
            Point mouseDownPoint = CurrentMousePosition(e);

            if ((e.ChangedButton == MouseButton.Left) && !sequencerDimensionsCalculator.IsPointInsideNote(notes, mouseDownPoint)
                && (ViewModel.NoteAction == NoteAction.Select))
            {
                DragSelectionBox.SetNewOriginMousePosition(mouseDownPoint);
            }

            MousePointNoteCommand noteCommand = mousePointNoteCommandFactory.FindCommand(ViewModel.NoteAction);
            command = new MoveNoteFromPointCommand(mouseDownPoint, notes, sequencerSettings, sequencerDimensionsCalculator);
            noteCommand.Execute(mouseDownPoint);

            SequencerGrid.CaptureMouse();

            e.Handled = true;
        }

        private void SequencerMouseMoved(object sender, MouseEventArgs e)
        {
            Point currentMousePosition = CurrentMousePosition(e);

            if (ViewModel.NoteAction == NoteAction.Create)
            {
                updateNoteEndPositionFromPointCommand.Execute(currentMousePosition);
            }
            if (ViewModel.NoteAction == NoteAction.Select)
            {
                var noteUnderMouse = sequencerDimensionsCalculator.FindNoteFromPoint(notes, currentMousePosition);
                
                DragSelectionBox.UpdateDragSelectionBox(currentMousePosition);

                if (!DragSelectionBox.IsDragging)
                {
                    Mouse.OverrideCursor = noteUnderMouse != null ? Cursors.Hand : null;
                    command?.Execute(currentMousePosition);
                }
                else
                {
                    IEnumerable<VisualNote> containedNotes = DragSelectionBox.FindMatches(notes);
                    selectNoteCommand.Execute(containedNotes);
                }
            }

            e.Handled = true;
        }

        private Point CurrentMousePosition(MouseEventArgs mouseEventArgs)
        {
            return mouseEventArgs.GetPosition(SequencerCanvas);
        }

        private void SequencerKeyPressed(object sender, KeyEventArgs e)
        {
            keyPressHandler.HandleSequencerKeyPressed(e.Key);
        }

        private void SequencerMouseUp(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                DragSelectionBox.CloseSelectionBox();
            }

            SequencerGrid.ReleaseMouseCapture();
            e.Handled = true;
        }
    }
}