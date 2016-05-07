using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using JetBrains.Annotations;
using log4net;
using Sequencer.Command;

namespace Sequencer
{
    public partial class MainWindow : INotifyPropertyChanged
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(MainWindow));
        private readonly CreateNoteCommand createNoteCommand;
        private readonly UpdateNoteEndPositionCommand updateNoteEndPositionCommand;

        private readonly List<VisualNote> notes = new List<VisualNote>();
        private readonly SequencerDimensionsCalculator sequencerDimensionsCalculator;
        private readonly SequencerSettings sequencerSettings = new SequencerSettings();
        private VisualNote currentNote;
        private NoteAction noteAction;

        public MainWindow()
        {
            InitializeComponent();
            Loaded += WindowChanged;
            SizeChanged += WindowChanged;
            sequencerDimensionsCalculator = new SequencerDimensionsCalculator(SequencerCanvas, sequencerSettings);
            createNoteCommand = new CreateNoteCommand(sequencerSettings, sequencerDimensionsCalculator);
            updateNoteEndPositionCommand = new UpdateNoteEndPositionCommand(sequencerSettings, sequencerDimensionsCalculator);
            Log.Info("Main Window loaded");
        }

        public NoteAction NoteAction
        {
            get { return noteAction; }
            set
            {
                noteAction = value;
                OnPropertyChanged(nameof(NoteAction));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void WindowChanged(object sender, RoutedEventArgs e)
        {
            RedrawEditor();
        }

        private void RedrawEditor()
        {
            SequencerCanvas.Children.Clear();

            DrawHorizontalSequencerLines();
            DrawVerticalSequencerLines();
            DrawNotes();

            Log.Debug("Sequencer redrawn");
        }

        private void DrawNotes()
        {
            foreach (VisualNote note in notes)
            {
                DrawNote(note);
            }
        }

        private void DrawNote(VisualNote visualNote)
        {
            visualNote.Draw(sequencerDimensionsCalculator, sequencerSettings, SequencerCanvas);
        }

        private void DrawVerticalSequencerLines()
        {
            double pointsPerMeasure = SequencerCanvas.ActualWidth/SequencerSettings.TotalMeasures;
            double pointsPerBar = pointsPerMeasure/sequencerSettings.TimeSignature.BarsPerMeasure;
            double pointsPerBeat = pointsPerBar/sequencerSettings.TimeSignature.BeatsPerBar;

            for (var measure = 0; measure < SequencerSettings.TotalMeasures; measure++)
            {
                double currentMeasurePosition = pointsPerMeasure*measure;
                DrawVerticalSequencerLine(currentMeasurePosition, 2);

                for (var bar = 0; bar < sequencerSettings.TimeSignature.BarsPerMeasure; bar++)
                {
                    double currentBarPosition = currentMeasurePosition + pointsPerBar*bar;
                    DrawVerticalSequencerLine(currentBarPosition, 1);

                    for (var beat = 1; beat < sequencerSettings.TimeSignature.BeatsPerBar; beat++)
                    {
                        double currentBeatPosition = currentBarPosition + pointsPerBeat*beat;
                        DrawVerticalSequencerLine(currentBeatPosition, 0.5);
                    }
                }
            }
        }

        private void DrawVerticalSequencerLine(double currentBeatPosition, double thickness)
        {
            var sequencerLine = new Line
            {
                X1 = currentBeatPosition,
                X2 = currentBeatPosition,
                Y1 = 0,
                Y2 = SequencerCanvas.ActualHeight,
                StrokeThickness = thickness,
                Stroke = new SolidColorBrush(sequencerSettings.LineColour)
            };

            SequencerCanvas.Children.Add(sequencerLine);
        }

        private void DrawHorizontalSequencerLines()
        {
            double pointsPerNote = sequencerDimensionsCalculator.NoteHeight;

            for (int currentNote = SequencerSettings.TotalNotes; currentNote >= 0; currentNote--)
            {
                double currentNotePosition = pointsPerNote*currentNote;

                int currentMidiNote = SequencerSettings.TotalNotes + sequencerSettings.LowestPitch.MidiNoteNumber - currentNote;
                Pitch pitch = Pitch.CreatePitchFromMidiNumber(currentMidiNote - 1);

                DrawNoteBackground(currentNotePosition, pointsPerNote, pitch);
                DrawHorizontalSequencerLine(currentNotePosition, 0.5);
            }
        }

        private void DrawNoteBackground(double currentNotePosition, double noteSize, Pitch pitch)
        {
            Color backgroundColour = pitch.Note.IsAccidental ? sequencerSettings.AccidentalKeyColour : sequencerSettings.KeyColour;

            var noteBackground = new Rectangle
            {
                Width = SequencerCanvas.ActualWidth,
                Height = noteSize,
                Fill = new SolidColorBrush(backgroundColour)
            };

            SequencerCanvas.Children.Add(noteBackground);
            Canvas.SetLeft(noteBackground, 0);
            Canvas.SetTop(noteBackground, currentNotePosition);
        }

        private void DrawHorizontalSequencerLine(double currentNotePosition, double thickness)
        {
            var sequencerLine = new Line
            {
                X1 = 0,
                X2 = SequencerCanvas.ActualWidth,
                Y1 = currentNotePosition,
                Y2 = currentNotePosition,
                StrokeThickness = thickness,
                Stroke = new SolidColorBrush(sequencerSettings.LineColour)
            };

            SequencerCanvas.Children.Add(sequencerLine);
        }

        private void SequencerMouseMoved(object sender, MouseEventArgs e)
        {
            if (NoteAction == NoteAction.Create && notes != null && Mouse.RightButton == MouseButtonState.Pressed)
            {
                Point mousePosition = CurrentMousePosition(e);
                updateNoteEndPositionCommand.UpdateNoteEndPosition(currentNote, mousePosition);
            }
        }
        
        private void SequencerMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (NoteAction == NoteAction.Create)
            {
                Point mousePosition = CurrentMousePosition(e);
                VisualNote note = createNoteCommand.CreateNote(mousePosition);
                note.Draw(sequencerDimensionsCalculator, sequencerSettings, SequencerCanvas);
                notes.Add(note);
                currentNote = note;
            }
        }

        private Point CurrentMousePosition(MouseEventArgs mouseEventArgs)
        {
            return mouseEventArgs.GetPosition(SequencerCanvas);
        }

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}