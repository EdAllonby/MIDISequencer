using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using log4net;

namespace Sequencer
{
    public partial class MainWindow
    {
        private const int MeasuresToDisplay = 4;
        private const double NotesToDisplay = 32;
        private static readonly ILog Log = LogManager.GetLogger(typeof (MainWindow));
        private readonly Dictionary<int, Note> notesIndexedById = new Dictionary<int, Note>();
        private readonly TimeSignature sequencerTimeSignature = new TimeSignature(4, 4);
        private int currentNoteId;
        private int noteId = 1;
        
        public MainWindow()
        {
            InitializeComponent();
            Loaded += WindowChanged;
            SizeChanged += WindowChanged;
            Log.Info("Main Window loaded");
        }

        private double NoteHeight => SequencerCanvas.ActualHeight/NotesToDisplay;

        private double BeatWidth => SequencerCanvas.ActualWidth/(sequencerTimeSignature.BeatsPerMeasure*MeasuresToDisplay);

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
            foreach (Note note in notesIndexedById.Values)
            {
                DrawNote(note);
            }
        }

        private void DrawNote(Note note)
        {
            note.DrawNote(sequencerTimeSignature, NoteHeight, BeatWidth, SequencerCanvas);
        }

        private void DrawVerticalSequencerLines()
        {
            double pointsPerMeasure = SequencerCanvas.ActualWidth/MeasuresToDisplay;
            double pointsPerBar = pointsPerMeasure/sequencerTimeSignature.BarsPerMeasure;
            double pointsPerBeat = pointsPerBar/sequencerTimeSignature.BeatsPerBar;

            for (var measure = 0; measure < MeasuresToDisplay; measure++)
            {
                double currentMeasurePosition = pointsPerMeasure*measure;
                DrawVerticalSequencerLine(currentMeasurePosition, 2, Colors.Black);

                for (var bar = 0; bar < sequencerTimeSignature.BarsPerMeasure; bar++)
                {
                    double currentBarPosition = currentMeasurePosition + pointsPerBar*bar;
                    DrawVerticalSequencerLine(currentBarPosition, 1, Colors.Black);

                    for (var beat = 1; beat < sequencerTimeSignature.BeatsPerBar; beat++)
                    {
                        double currentBeatPosition = currentBarPosition + pointsPerBeat*beat;
                        DrawVerticalSequencerLine(currentBeatPosition, 0.5, Colors.Black);
                    }
                }
            }
        }

        private void DrawVerticalSequencerLine(double currentBeatPosition, double thickness, Color colour)
        {
            var sequencerLine = new Line
            {
                X1 = currentBeatPosition,
                X2 = currentBeatPosition,
                Y1 = 0,
                Y2 = SequencerCanvas.ActualHeight,
                StrokeThickness = thickness,
                Stroke = new SolidColorBrush(colour)
            };

            SequencerCanvas.Children.Add(sequencerLine);
        }

        private void DrawHorizontalSequencerLines()
        {
            double pointsPerNote = NoteHeight;

            var isAlternate = false;
            for (var currentNote = 0; currentNote < NotesToDisplay; currentNote++)
            {
                double currentNotePosition = pointsPerNote*currentNote;
                isAlternate = !isAlternate;

                DrawNoteBackground(currentNotePosition, pointsPerNote, isAlternate);
                DrawHorizontalSequencerLine(currentNotePosition, 0.5, Colors.Black);
            }
        }

        private void DrawNoteBackground(double currentNotePosition, double noteSize, bool isAlternate)
        {
            Color backgroundColour = isAlternate ? Colors.Gray : Colors.DarkGray;

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

        private void DrawHorizontalSequencerLine(double currentNotePosition, double thickness, Color colour)
        {
            var sequencerLine = new Line
            {
                X1 = 0,
                X2 = SequencerCanvas.ActualWidth,
                Y1 = currentNotePosition,
                Y2 = currentNotePosition,
                StrokeThickness = thickness,
                Stroke = new SolidColorBrush(colour)
            };

            SequencerCanvas.Children.Add(sequencerLine);
        }

        private void SequencerMouseMoved(object sender, MouseEventArgs e)
        {
            if (notesIndexedById != null && Mouse.RightButton == MouseButtonState.Pressed)
            {
                UpdateNote(e);
            }
        }

        private void UpdateNote(MouseEventArgs e)
        {
            Point mousePosition = e.GetPosition(SequencerCanvas);
            Position endPosition = FindNotePosition(mousePosition);
            notesIndexedById[currentNoteId].UpdateNoteLength(sequencerTimeSignature, endPosition.NextPosition(sequencerTimeSignature), BeatWidth);
        }

        private void SequencerMouseDown(object sender, MouseButtonEventArgs e)
        {
            Point mousePosition = e.GetPosition(SequencerCanvas);
            Position notePosition = FindNotePosition(mousePosition);
            int noteValue = FindNote(mousePosition);

            var note = new Note(noteId, notePosition, notePosition.NextPosition(sequencerTimeSignature),  noteValue);
            DrawNote(note);
            notesIndexedById.Add(noteId, note);

            currentNoteId = noteId;
            noteId++;
        }

        private Position FindNotePosition(Point mousePosition)
        {
            var beat = (int) Math.Ceiling(mousePosition.X/BeatWidth);
            return Position.PositionFromBeat(beat, sequencerTimeSignature);
        }

        private void SequencerMouseUp(object sender, MouseButtonEventArgs e)
        {
            UpdateNote(e);
        }

        private int FindNote(Point mousePosition)
        {
            return (int)Math.Ceiling(mousePosition.Y / NoteHeight);
        }
    }
}