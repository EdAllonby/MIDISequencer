using System.Collections.Generic;
using Avalonia;
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
    public class PositionIndicatorHeaderDrawer
    {
        private const double IndicatorHeaderSize = 10;
        [NotNull] private static readonly ILog Log = LogExtensions.GetLoggerSafe(typeof(PositionIndicatorHeaderDrawer));
        [NotNull] private readonly Polygon indicatorHeader;

        [NotNull] private readonly ISequencerDimensionsCalculator sequencerDimensionsCalculator;
        [NotNull] private readonly ISequencerCanvasWrapper sequencerHeaderCanvas;

        public PositionIndicatorHeaderDrawer([NotNull] IColourSettings sequencerSettings,
            [NotNull] ISequencerCanvasWrapper sequencerHeaderCanvas, [NotNull] ISequencerDimensionsCalculator sequencerDimensionsCalculator)
        {
            this.sequencerHeaderCanvas = sequencerHeaderCanvas;
            this.sequencerDimensionsCalculator = sequencerDimensionsCalculator;

            var indicatorBrush = sequencerSettings.IndicatorColour.ToBrush();

            indicatorHeader = new Polygon
            {
                Points = new List<Point> { new Point(0, 0), new Point(IndicatorHeaderSize / 2.0, IndicatorHeaderSize), new Point(IndicatorHeaderSize, 0) },
                Fill = indicatorBrush,
                ZIndex = int.MaxValue
            };

            sequencerHeaderCanvas.AddChild(indicatorHeader);


            DrawPositionIndicator(CurrentPosition);
        }

        [NotNull]
        private IPosition CurrentPosition { get; set; } = new Position(1, 1, 1);

        public void DrawPositionIndicator([NotNull] IPosition position)
        {
            Log.Debug($"New Indicator Header Position drawing: {position}");

            CurrentPosition = position;
            double noteStartLocation = sequencerDimensionsCalculator.GetPointFromPosition(position);

            SetIndicatorHeaderPosition(noteStartLocation);
        }

        public void RedrawEditor()
        {
            DrawPositionIndicator(CurrentPosition);
        }

        private void SetIndicatorHeaderPosition(double position)
        {
            Canvas.SetLeft(indicatorHeader, position - IndicatorHeaderSize / 2);
            Canvas.SetTop(indicatorHeader, sequencerHeaderCanvas.Height - IndicatorHeaderSize);
        }
    }
}