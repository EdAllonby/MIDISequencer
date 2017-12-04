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
        private const double IndicatorWidth = 2;
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
                Fill = new SolidColorBrush(sequencerSettings.IndicatorColour)
            };

            sequencerCanvas.AddChild(indicator);

            Panel.SetZIndex(indicator, int.MaxValue);

            DrawPositionIndicator(CurrentPosition);
        }

        [NotNull]
        private IPosition CurrentPosition { get; set; } = new Position(1, 1, 1);

        public void DrawPositionIndicator([NotNull] IPosition position)
        {
            Log.Info($"New Indicator Position drawing: {position}");

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
            Canvas.SetLeft(indicator, position - IndicatorWidth / 2);
            Canvas.SetTop(indicator, 0);
        }
    }
}