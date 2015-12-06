using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Sequencer
{
    public partial class MainWindow
    {
        public readonly int BarsPerMeasure = 4;

        public readonly int BeatsPerBar = 4;
        public readonly int MeasuresToDisplay = 4;
        private readonly Dictionary<int, Note> notesIndexedById = new Dictionary<int, Note>();
        public readonly double NotesToDisplay = 32;
        private int currentNoteId;
        private int noteId = 1;


        public MainWindow()
        {
            InitializeComponent();
            Loaded += WindowChanged;
            SizeChanged += WindowChanged;
        }

        private double NoteHeight => SequencerCanvas.ActualHeight/NotesToDisplay;

        private double BeatWidth => SequencerCanvas.ActualWidth/(BeatsPerBar*BarsPerMeasure*MeasuresToDisplay);

        private void WindowChanged(object sender, RoutedEventArgs e)
        {
            DrawEditor();
        }

        private void DrawEditor()
        {
            SequencerCanvas.Children.Clear();

            DrawHorizontalSequencerLines();
            DrawVerticalSequencerLines();
            DrawNotes();
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
            note.DrawNote(NoteHeight, BeatWidth, SequencerCanvas);
        }

        private void DrawVerticalSequencerLines()
        {
            double pointsPerMeasure = SequencerCanvas.ActualWidth/MeasuresToDisplay;
            double pointsPerBar = pointsPerMeasure/BarsPerMeasure;
            double pointsPerBeat = pointsPerBar/BeatsPerBar;

            for (int measure = 0; measure < MeasuresToDisplay; measure++)
            {
                double currentMeasurePosition = (pointsPerMeasure*measure);
                DrawVerticalSequencerLine(currentMeasurePosition, 2, Colors.Black);

                for (int bar = 0; bar < BarsPerMeasure; bar++)
                {
                    double currentBarPosition = currentMeasurePosition + (pointsPerBar*bar);
                    DrawVerticalSequencerLine(currentBarPosition, 1, Colors.Black);

                    for (int beat = 1; beat < BeatsPerBar; beat++)
                    {
                        double currentBeatPosition = currentBarPosition + (pointsPerBeat*beat);
                        DrawVerticalSequencerLine(currentBeatPosition, 0.5, Colors.Black);
                    }
                }
            }
        }

        private void DrawVerticalSequencerLine(double currentBeatPosition, double thickness, Color colour)
        {
            Line sequencerLine = new Line
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

            bool isAlternate = false;
            for (int currentNote = 0; currentNote < NotesToDisplay; currentNote++)
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

            Rectangle noteBackground = new Rectangle
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
            Line sequencerLine = new Line
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
            notesIndexedById[currentNoteId].UpdateNoteLength(endPosition, BeatWidth);
        }

        private void SequencerMouseDown(object sender, MouseButtonEventArgs e)
        {
            Point mousePoint = e.GetPosition(SequencerCanvas);
            Position notePosition = FindNotePosition(mousePoint);
            int noteValue = FindNote(mousePoint);

            var note = new Note(noteId, notePosition, noteValue);
            DrawNote(note);
            notesIndexedById.Add(noteId, note);

            currentNoteId = noteId;
            noteId++;
        }

        private void SequencerMouseUp(object sender, MouseButtonEventArgs e)
        {
            UpdateNote(e);
        }

        private Position FindNotePosition(Point mousePosition)
        {
            double xPosition = mousePosition.X;

            for (int measure = 1; measure <= MeasuresToDisplay; measure++)
            {
                for (int bar = 1; bar <= BarsPerMeasure; bar++)
                {
                    for (int beat = 1; beat <= BeatsPerBar; beat++)
                    {
                        var possiblePosition = new Position(measure, bar, beat);

                        double currentBeatPosition = possiblePosition.SummedBeat(BarsPerMeasure, BeatsPerBar);

                        if (xPosition <= BeatWidth*currentBeatPosition)
                        {
                            return new Position(measure, bar, beat);
                        }
                    }
                }
            }

            return new Position(0, 0, 0);
        }

        private int FindNote(Point mousePosition)
        {
            double yPosition = mousePosition.Y;

            for (int note = 1; note <= NotesToDisplay; note++)
            {
                if (yPosition <= NoteHeight*note)
                {
                    return note;
                }
            }

            return 0;
        }
    }
}