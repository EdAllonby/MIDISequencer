using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using JetBrains.Annotations;
using log4net;
using Sequencer.Command;

namespace Sequencer
{
    public partial class MainWindow : INotifyPropertyChanged
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(MainWindow));
        private readonly DeleteNotesCommand deleteNotesCommand;
        private readonly UpdateNoteStateCommand selectNoteCommand;

        private readonly MousePointNoteCommandFactory mousePointNoteCommandFactory;

        private readonly List<VisualNote> notes = new List<VisualNote>();
        private readonly SequencerDimensionsCalculator sequencerDimensionsCalculator;
        private readonly SequencerDrawer sequencerDrawer;
        private readonly SequencerSettings sequencerSettings = new SequencerSettings();
        private readonly UpdateNoteEndPositionFromPointCommand updateNoteEndPositionFromPointCommand;
        private MousePointMoveNoteCommand moveNoteCommand;
        private NoteAction noteAction;

        public MainWindow()
        {
            InitializeComponent();
            SizeChanged += WindowChanged;
            sequencerDimensionsCalculator = new SequencerDimensionsCalculator(SequencerCanvas, sequencerSettings);
            mousePointNoteCommandFactory = new MousePointNoteCommandFactory(SequencerCanvas, notes, sequencerSettings, sequencerDimensionsCalculator);
            updateNoteEndPositionFromPointCommand = new UpdateNoteEndPositionFromPointCommand(notes, sequencerSettings, sequencerDimensionsCalculator);
            deleteNotesCommand = new DeleteNotesCommand(SequencerCanvas, notes);
            selectNoteCommand = new UpdateNoteStateCommand(notes, NoteState.Selected);
            sequencerDrawer = new SequencerDrawer(SequencerCanvas, notes, sequencerDimensionsCalculator, sequencerSettings);

            Log.Info("Main Window loaded");
        }

        public NoteAction NoteAction
        {
            get { return noteAction; }
            set
            {
                noteAction = value;
                OnPropertyChanged(nameof(NoteAction));
                Log.Info($"Note action set to {NoteAction}");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void WindowChanged(object sender, RoutedEventArgs e)
        {
            sequencerDrawer.RedrawEditor();
        }
        
        private void SequencerMouseDown(object sender, MouseButtonEventArgs e)
        {
            Point mousePosition = CurrentMousePosition(e);

            MousePointNoteCommand noteCommand = mousePointNoteCommandFactory.FindCommand(NoteAction);
            moveNoteCommand = new MousePointMoveNoteCommand(mousePosition, notes, sequencerSettings, sequencerDimensionsCalculator);
            noteCommand.Execute(mousePosition);
        }

        private void SequencerMouseMoved(object sender, MouseEventArgs e)
        {
            Point currentMousePosition = CurrentMousePosition(e);

            if (NoteAction == NoteAction.Create)
            {
                updateNoteEndPositionFromPointCommand.Execute(currentMousePosition);
            }
            if (NoteAction == NoteAction.Select)
            {
                moveNoteCommand?.Execute(currentMousePosition);
            }
        }

        private Point CurrentMousePosition(MouseEventArgs mouseEventArgs)
        {
            return mouseEventArgs.GetPosition(SequencerCanvas);
        }

        [NotifyPropertyChangedInvocator]
        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void SequencerKeyPressed(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Delete)
            {
                deleteNotesCommand.Execute(notes.Where(x => x.NoteState == NoteState.Selected));
                e.Handled = true;
            }
            if (Keyboard.IsKeyDown(Key.LeftCtrl) && (e.Key == Key.A))
            {
                selectNoteCommand.Execute(notes);
            }
        }
    }
}