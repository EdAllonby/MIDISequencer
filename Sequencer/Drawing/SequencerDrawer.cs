using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using JetBrains.Annotations;
using log4net;
using Sequencer.Domain;
using Sequencer.View;

namespace Sequencer.Drawing
{
    public sealed class SequencerDrawer
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(SequencerDrawer));
        private readonly List<UIElement> elementCache = new List<UIElement>();
        private readonly IDigitalAudioProtocol protocol;

        private readonly Canvas sequencerCanvas;
        private readonly SequencerDimensionsCalculator sequencerDimensionsCalculator;
        private readonly ISequencerNotes sequencerNotes;
        private readonly SequencerSettings sequencerSettings;

        public SequencerDrawer([NotNull] Canvas sequencerCanvas, [NotNull] ISequencerNotes sequencerNotes,
            [NotNull] SequencerDimensionsCalculator sequencerDimensionsCalculator, [NotNull] SequencerSettings sequencerSettings)
        {
            protocol = sequencerSettings.Protocol;
            this.sequencerCanvas = sequencerCanvas;
            this.sequencerNotes = sequencerNotes;
            this.sequencerDimensionsCalculator = sequencerDimensionsCalculator;
            this.sequencerSettings = sequencerSettings;
        }

        public void RedrawEditor()
        {
            RemoveElements();
            DrawHorizontalSequencerLines();
            DrawVerticalSequencerLines();
            AddElements();
            sequencerNotes.DrawNotes();

            Log.Debug("Sequencer redrawn");
        }

        private void DrawVerticalSequencerLines()
        {
            for (var measure = 0; measure < SequencerSettings.TotalMeasures; measure++)
            {
                double currentMeasurePosition = sequencerDimensionsCalculator.MeasureWidth*measure;
                DrawVerticalSequencerLine(currentMeasurePosition, 2);

                for (var bar = 0; bar < sequencerSettings.TimeSignature.BarsPerMeasure; bar++)
                {
                    double currentBarPosition = currentMeasurePosition + (sequencerDimensionsCalculator.BarWidth*bar);
                    DrawVerticalSequencerLine(currentBarPosition, 1);

                    for (var beat = 1; beat < sequencerSettings.TimeSignature.BeatsPerBar; beat++)
                    {
                        double currentBeatPosition = currentBarPosition + (sequencerDimensionsCalculator.BeatWidth*beat);
                        DrawVerticalSequencerLine(currentBeatPosition, 0.5);
                    }
                }
            }
        }

        private void DrawHorizontalSequencerLines()
        {
            double pointsPerNote = sequencerDimensionsCalculator.NoteHeight;

            for (int note = SequencerSettings.TotalNotes; note >= 0; note--)
            {
                double currentNotePosition = pointsPerNote*note;

                int currentMidiNote = (SequencerSettings.TotalNotes + protocol.ProtocolNoteNumber(sequencerSettings.LowestPitch)) - note;
                Pitch pitch = protocol.CreatePitchFromProtocolNumber(currentMidiNote - 1);

                DrawNoteBackground(currentNotePosition, pointsPerNote, pitch);
                DrawHorizontalSequencerLine(currentNotePosition, 0.5);
            }
        }

        private void DrawVerticalSequencerLine(double currentBeatPosition, double thickness)
        {
            var sequencerLine = new Line
            {
                X1 = currentBeatPosition,
                X2 = currentBeatPosition,
                Y1 = 0,
                Y2 = sequencerCanvas.ActualHeight,
                StrokeThickness = thickness,
                Stroke = new SolidColorBrush(sequencerSettings.LineColour)
            };

            elementCache.Add(sequencerLine);
        }

        private void DrawNoteBackground(double currentNotePosition, double noteSize, Pitch pitch)
        {
            Color backgroundColour = pitch.Note.IsAccidental ? sequencerSettings.AccidentalKeyColour : sequencerSettings.KeyColour;

            var noteBackground = new Rectangle
            {
                Width = sequencerCanvas.ActualWidth,
                Height = noteSize,
                Fill = new SolidColorBrush(backgroundColour)
            };

            Canvas.SetLeft(noteBackground, 0);
            Canvas.SetTop(noteBackground, currentNotePosition);

            elementCache.Add(noteBackground);
        }

        private void DrawHorizontalSequencerLine(double currentNotePosition, double thickness)
        {
            var sequencerLine = new Line
            {
                X1 = 0,
                X2 = sequencerCanvas.ActualWidth,
                Y1 = currentNotePosition,
                Y2 = currentNotePosition,
                StrokeThickness = thickness,
                Stroke = new SolidColorBrush(sequencerSettings.LineColour)
            };

            elementCache.Add(sequencerLine);
        }

        private void AddElements()
        {
            foreach (UIElement uiElement in elementCache)
            {
                sequencerCanvas.Children.Add(uiElement);
            }
        }

        private void RemoveElements()
        {
            foreach (UIElement uiElement in elementCache)
            {
                sequencerCanvas.Children.Remove(uiElement);
            }

            elementCache.Clear();
        }
    }
}