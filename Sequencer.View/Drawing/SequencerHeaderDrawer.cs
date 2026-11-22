using System;
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
    /// <summary>
    /// Draws the ruler: bar numbers, with ticks for bars, beats and sixteenths at decreasing heights.
    /// </summary>
    public class SequencerHeaderDrawer
    {
        [NotNull] [ItemNotNull] private readonly List<Avalonia.Controls.Control> elementCache = new List<Avalonia.Controls.Control>();
        [NotNull] private readonly ISequencerDimensionsCalculator sequencerDimensionsCalculator;
        [NotNull] private readonly ISequencerCanvasWrapper sequencerHeaderCanvas;
        [NotNull] private readonly SequencerSettings sequencerSettings;

        public SequencerHeaderDrawer([NotNull] ISequencerCanvasWrapper sequencerHeaderCanvas, [NotNull] ISequencerDimensionsCalculator sequencerDimensionsCalculator,
            [NotNull] SequencerSettings sequencerSettings)
        {
            this.sequencerHeaderCanvas = sequencerHeaderCanvas;
            this.sequencerDimensionsCalculator = sequencerDimensionsCalculator;
            this.sequencerSettings = sequencerSettings;
        }

        /// <summary>
        /// The number of the first bar shown, so a section later in the song shows its real bar numbers.
        /// </summary>
        public int FirstBarNumber { get; set; } = 1;

        public void RedrawHeader()
        {
            RemoveElements();
            DrawRuler();
            AddElements();
        }

        private void DrawRuler()
        {
            double height = sequencerHeaderCanvas.Height;
            IBrush tick = sequencerSettings.RulerTickColour.ToBrush();
            IBrush text = sequencerSettings.RulerTextColour.ToBrush();
            int sixteenthsPerBeat = 4;
            int sixteenthsPerBar = sixteenthsPerBeat * sequencerSettings.TimeSignature.BeatsPerBar;
            int totalSixteenths = sixteenthsPerBar * sequencerSettings.TimeSignature.BarsPerMeasure * sequencerSettings.TotalMeasures;

            for (var step = 0; step < totalSixteenths; step++)
            {
                double x = SequencerDrawer.Snap(step * sequencerDimensionsCalculator.SixteenthNoteWidth);
                bool isBar = step % sixteenthsPerBar == 0;
                bool isBeat = step % sixteenthsPerBeat == 0;
                double tickHeight = isBar ? height : isBeat ? height * 0.35 : height * 0.18;

                elementCache.Add(new Line
                {
                    StartPoint = new Point(x, height - tickHeight),
                    EndPoint = new Point(x, height),
                    StrokeThickness = 1,
                    Stroke = tick
                });

                if (isBar)
                {
                    var barNumber = new TextBlock
                    {
                        Text = (step / sixteenthsPerBar + FirstBarNumber).ToString(),
                        FontSize = 11,
                        FontWeight = FontWeight.SemiBold,
                        Foreground = text
                    };
                    Canvas.SetLeft(barNumber, x + 5);
                    Canvas.SetTop(barNumber, Math.Max(0, height / 2 - 12));
                    elementCache.Add(barNumber);
                }
            }

            // A hairline along the bottom separates the ruler from the grid.
            double bottom = SequencerDrawer.Snap(height - 1);
            elementCache.Add(new Line { StartPoint = new Point(0, bottom), EndPoint = new Point(sequencerHeaderCanvas.Width, bottom), StrokeThickness = 1, Stroke = tick });
        }

        private void AddElements()
        {
            foreach (Avalonia.Controls.Control element in elementCache)
            {
                sequencerHeaderCanvas.AddChild(element);
            }
        }

        private void RemoveElements()
        {
            foreach (Avalonia.Controls.Control element in elementCache)
            {
                sequencerHeaderCanvas.RemoveChild(element);
            }

            elementCache.Clear();
        }
    }
}
