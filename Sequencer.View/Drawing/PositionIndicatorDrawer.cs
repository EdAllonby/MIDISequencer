using System;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using JetBrains.Annotations;
using log4net;
using Sequencer.Domain;
using Sequencer.Domain.Settings;
using Sequencer.Utilities;
using Sequencer.Visual;

namespace Sequencer.View.Drawing
{
    public class PositionIndicatorDrawer
    {
        private const double IndicatorWidth = 1;
        [NotNull] private static readonly ILog Log = LogExtensions.GetLoggerSafe(typeof(PositionIndicatorDrawer));
        [NotNull] private readonly Rectangle indicator;
        [NotNull] private readonly ISequencerCanvasWrapper sequencerCanvas;
        [NotNull] private readonly ISequencerDimensionsCalculator sequencerDimensionsCalculator;

        public PositionIndicatorDrawer([NotNull] SequencerSettings sequencerSettings, [NotNull] ISequencerCanvasWrapper sequencerCanvas,
            [NotNull] ISequencerDimensionsCalculator sequencerDimensionsCalculator)
        {
            this.sequencerCanvas = sequencerCanvas;
            this.sequencerDimensionsCalculator = sequencerDimensionsCalculator;

            indicator = new Rectangle
            {
                StrokeThickness = 0.5,
                Width = IndicatorWidth,
                Fill = sequencerSettings.PlayheadColour.ToBrush(),
                ZIndex = int.MaxValue
            };

            sequencerCanvas.AddChild(indicator);


            DrawPositionIndicator(CurrentPosition);
        }

        [NotNull]
        private IPosition CurrentPosition { get; set; } = new Position(1, 1, 1);

        public void DrawPositionIndicator([NotNull] IPosition position)
        {
            Log.Debug($"New Indicator Position drawing: {position}");

            indicator.Height = sequencerCanvas.Height;

            CurrentPosition = position;
            double noteStartLocation = sequencerDimensionsCalculator.GetPointFromPosition(position);

            SetIndicatorPosition(noteStartLocation);
        }

        public void RedrawEditor()
        {
            DrawPositionIndicator(CurrentPosition);
        }

        private void SetIndicatorPosition(double position)
        {
            Canvas.SetLeft(indicator, Math.Floor(position));
            Canvas.SetTop(indicator, 0);
        }
    }
}