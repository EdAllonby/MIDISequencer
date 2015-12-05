using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Sequencer
{
    public partial class MainWindow
    {
        private const int BeatsPerBar = 4;
        private const int BarsPerMeasure = 4;
        private const int MeasuresToDisplay = 4;
        private const double NotesToDisplay = 32;

        public MainWindow()
        {
            InitializeComponent();
            Loaded += WindowChanged;
            SizeChanged += WindowChanged;
        }

        private void WindowChanged(object sender, RoutedEventArgs e)
        {
            DrawEditor();
        }

        private void DrawEditor()
        {
            Sequencer.Children.Clear();

            DrawHorizontalSequencerLines();
            DrawVerticalSequencerLines();
        }

        private void DrawVerticalSequencerLines()
        {
            double pointsPerMeasure = Sequencer.ActualWidth/MeasuresToDisplay;
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
                Y2 = Sequencer.ActualHeight,
                StrokeThickness = thickness,
                Stroke = new SolidColorBrush(colour)
            };

            Sequencer.Children.Add(sequencerLine);
        }

        private void DrawHorizontalSequencerLines()
        {
            double pointsPerNote = Sequencer.ActualHeight/NotesToDisplay;

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
                Width = Sequencer.ActualWidth,
                Height = noteSize,
                Fill = new SolidColorBrush(backgroundColour)
            };

            Sequencer.Children.Add(noteBackground);
            Canvas.SetLeft(noteBackground, 0);
            Canvas.SetTop(noteBackground, currentNotePosition);
        }

        private void DrawHorizontalSequencerLine(double currentNotePosition, double thickness, Color colour)
        {
            Line sequencerLine = new Line
            {
                X1 = 0,
                X2 = Sequencer.ActualWidth,
                Y1 = currentNotePosition,
                Y2 = currentNotePosition,
                StrokeThickness = thickness,
                Stroke = new SolidColorBrush(colour)
            };

            Sequencer.Children.Add(sequencerLine);
        }
    }
}