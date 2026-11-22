using System.Collections.Generic;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Media;
using JetBrains.Annotations;
using Sequencer.Domain.Settings;
using Sequencer.Visual;

namespace Sequencer.View.Drawing
{
    public class SequencerHeaderDrawer
    {
        [NotNull] [ItemNotNull] private readonly List<Avalonia.Controls.Control> elementCache = new List<Avalonia.Controls.Control>();
        [NotNull] private readonly ISequencerDimensionsCalculator sequencerDimensionsCalculator;
        private readonly ISequencerCanvasWrapper sequencerHeaderCanvas;
        [NotNull] private readonly SequencerSettings sequencerSettings;

        public SequencerHeaderDrawer(ISequencerCanvasWrapper sequencerHeaderCanvas, [NotNull] ISequencerDimensionsCalculator sequencerDimensionsCalculator, [NotNull] SequencerSettings sequencerSettings)
        {
            this.sequencerHeaderCanvas = sequencerHeaderCanvas;
            this.sequencerDimensionsCalculator = sequencerDimensionsCalculator;
            this.sequencerSettings = sequencerSettings;
        }

        public void RedrawHeader()
        {
            RemoveElements();
            DrawVerticalSequencerLine();
            AddElements();
        }

        private void DrawVerticalSequencerLine()
        {
            var totalBeats = 0;
            for (var measure = 0; measure < sequencerSettings.TotalMeasures; measure++)
            {
                double currentMeasurePosition = sequencerDimensionsCalculator.MeasureWidth * measure;

                if (measure != 0)
                {
                    DrawVerticalSequencerLine(currentMeasurePosition, 1);
                }

                for (var bar = 0; bar < sequencerSettings.TimeSignature.BarsPerMeasure; bar++)
                {
                    double currentBarPosition = currentMeasurePosition + sequencerDimensionsCalculator.BarWidth * bar;

                    if (bar != 0)
                    {
                        DrawVerticalSequencerLine(currentBarPosition, 1);
                    }

                    for (var beat = 0; beat < sequencerSettings.TimeSignature.BeatsPerBar; beat++)
                    {
                        totalBeats++;
                        double currentBeatPosition = currentBarPosition + sequencerDimensionsCalculator.BeatWidth * beat;

                        if (beat != 0)
                        {
                            DrawVerticalSequencerLine(currentBeatPosition, 1);
                        }

                        DrawBeatNumber(currentBeatPosition, totalBeats);

                        for (var sixteenthNote = 0; sixteenthNote < 4; sixteenthNote++)
                        {
                            double currentEighthNotePosition = currentBeatPosition + sequencerDimensionsCalculator.SixteenthNoteWidth * sixteenthNote;

                            if (sixteenthNote != 0)
                            {
                                DrawVerticalSequencerLine(currentEighthNotePosition, 2);
                            }
                        }
                    }
                }
            }
        }

        private void DrawBeatNumber(double currentPosition, int currentBeat)
        {
            var textBlock = new TextBlock { Text = currentBeat.ToString(), FontSize = 11, Foreground = Brushes.Black };

            Canvas.SetLeft(textBlock, currentPosition + 2);
            Canvas.SetTop(textBlock, 0);

            elementCache.Add(textBlock);
        }

        private void DrawVerticalSequencerLine(double currentPosition, double heightModifier)
        {
            var sequencerLine = new Line
            {
                StartPoint = new Point(currentPosition, sequencerHeaderCanvas.Height - sequencerHeaderCanvas.Height / heightModifier),
                EndPoint = new Point(currentPosition, sequencerHeaderCanvas.Height),
                StrokeThickness = 1,
                Stroke = sequencerSettings.LineColour.ToBrush()
            };

            elementCache.Add(sequencerLine);
        }

        private void AddElements()
        {
            foreach (Avalonia.Controls.Control uiElement in elementCache)
            {
                sequencerHeaderCanvas.AddChild(uiElement);
            }
        }

        private void RemoveElements()
        {
            foreach (Avalonia.Controls.Control uiElement in elementCache)
            {
                sequencerHeaderCanvas.RemoveChild(uiElement);
            }

            elementCache.Clear();
        }
    }
}