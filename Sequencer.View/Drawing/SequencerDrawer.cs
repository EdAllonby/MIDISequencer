using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using JetBrains.Annotations;
using log4net;
using Sequencer.Domain;
using Sequencer.Midi;
using Sequencer.Shared;
using Sequencer.Utilities;
using Sequencer.View.Control;

namespace Sequencer.View.Drawing
{
    public sealed class SequencerDrawer
    {
        [NotNull] private static readonly ILog Log = LogExtensions.GetLoggerSafe(typeof(SequencerDrawer));
        [NotNull] [ItemNotNull] private readonly List<UIElement> elementCache = new List<UIElement>();
        [NotNull] private readonly IDigitalAudioProtocol protocol;

        [NotNull] private readonly ISequencerCanvasWrapper sequencerCanvas;
        [NotNull] private readonly ISequencerDimensionsCalculator sequencerDimensionsCalculator;
        [NotNull] private readonly ISequencerNotes sequencerNotes;
        [NotNull] private readonly SequencerSettings sequencerSettings;

        public SequencerDrawer([NotNull] IDigitalAudioProtocol protocol, [NotNull] ISequencerCanvasWrapper sequencerCanvas, [NotNull] ISequencerNotes sequencerNotes,
            [NotNull] ISequencerDimensionsCalculator sequencerDimensionsCalculator, [NotNull] SequencerSettings sequencerSettings)
        {
            this.protocol = protocol;
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
            for (var measure = 0; measure < sequencerSettings.TotalMeasures; measure++)
            {
                double currentMeasurePosition = sequencerDimensionsCalculator.MeasureWidth * measure;
                DrawVerticalSequencerLine(currentMeasurePosition, 4);

                for (var bar = 0; bar < sequencerSettings.TimeSignature.BarsPerMeasure; bar++)
                {
                    double currentBarPosition = currentMeasurePosition + sequencerDimensionsCalculator.BarWidth * bar;
                    DrawVerticalSequencerLine(currentBarPosition, 2);

                    for (var beat = 0; beat < sequencerSettings.TimeSignature.BeatsPerBar; beat++)
                    {
                        double currentBeatPosition = currentBarPosition + sequencerDimensionsCalculator.BeatWidth * beat;
                        DrawVerticalSequencerLine(currentBeatPosition, 1);

                        for (var sixteenthNote = 0; sixteenthNote < 4; sixteenthNote++)
                        {
                            double currentEighthNotePosition = currentBeatPosition + sequencerDimensionsCalculator.SixteenthNoteWidth * sixteenthNote;
                            DrawVerticalSequencerLine(currentEighthNotePosition, 0.5);
                        }
                    }
                }
            }
        }

        private void DrawHorizontalSequencerLines()
        {
            double pointsPerNote = sequencerDimensionsCalculator.NoteHeight;

            for (int note = sequencerSettings.TotalNotes - 1; note >= 0; note--)
            {
                double currentNotePosition = pointsPerNote * note;

                int currentMidiNote = sequencerSettings.TotalNotes + protocol.ProtocolNoteNumber(sequencerSettings.LowestPitch) - note;
                Pitch pitch = protocol.CreatePitchFromProtocolNumber(currentMidiNote - 1);

                DrawNoteLaneBackground(currentNotePosition, pointsPerNote, pitch);
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
                Y2 = sequencerCanvas.Height,
                StrokeThickness = thickness,
                Stroke = new SolidColorBrush(sequencerSettings.LineColour)
            };

            elementCache.Add(sequencerLine);
        }

        private void DrawNoteLaneBackground(double currentNotePosition, double noteSize, [NotNull] Pitch pitch)
        {
            Color backgroundColour = pitch.Note.IsAccidental ? sequencerSettings.AccidentalKeyColour : sequencerSettings.KeyColour;

            var noteBackground = new Rectangle
            {
                Width = sequencerCanvas.Width,
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
                X2 = sequencerCanvas.Width,
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
                sequencerCanvas.AddChild(uiElement);
            }
        }

        private void RemoveElements()
        {
            foreach (UIElement uiElement in elementCache)
            {
                sequencerCanvas.RemoveChild(uiElement);
            }

            elementCache.Clear();
        }
    }
}