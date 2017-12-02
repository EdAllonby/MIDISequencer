using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using JetBrains.Annotations;
using log4net;
using Sequencer.Domain;
using Sequencer.Shared;
using Sequencer.Utilities;

namespace Sequencer.View.Drawing
{
    public class PositionIndicatorDrawer
    {
        [NotNull] private static readonly ILog Log = LogExtensions.GetLoggerSafe(typeof(PositionIndicatorDrawer));
        [NotNull] private readonly Rectangle indicator;
        [NotNull] private readonly ISequencerCanvasWrapper sequencerCanvas;
        [NotNull] private readonly ISequencerDimensionsCalculator sequencerDimensionsCalculator;
        [NotNull] private IPosition currentPosition;
        private const double IndicatorWidth = 2;

        public PositionIndicatorDrawer([NotNull] SequencerSettings sequencerSettings, [NotNull] ISequencerCanvasWrapper sequencerCanvas, [NotNull] ISequencerDimensionsCalculator sequencerDimensionsCalculator)
        {
            this.sequencerCanvas = sequencerCanvas;
            this.sequencerDimensionsCalculator = sequencerDimensionsCalculator;

            currentPosition = new Position(0, 0, 0);

            indicator = new Rectangle
            {
                StrokeThickness = 0.5,
                Width = IndicatorWidth,
                Fill = new SolidColorBrush(sequencerSettings.IndicatorColour)
            };

            sequencerCanvas.AddChild(indicator);

            Panel.SetZIndex(indicator, int.MaxValue);

            DrawPositionIndicator(currentPosition);
        }

        public void DrawPositionIndicator([NotNull] IPosition position)
        {
            Log.Info($"New Indicator Position drawing: {position}");

            indicator.Height = sequencerCanvas.Height;

            currentPosition = position;
            double noteStartLocation = sequencerDimensionsCalculator.GetPointFromPosition(position);

            SetRectanglePosition(noteStartLocation);
        }

        public void RedrawEditor()
        {
            DrawPositionIndicator(currentPosition);
        }

        private void SetRectanglePosition(double position)
        {
            Canvas.SetLeft(indicator, position - IndicatorWidth / 2);
            Canvas.SetTop(indicator, 0);
        }
    }
}