using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using JetBrains.Annotations;
using Sequencer.Domain;
using Sequencer.Domain.Settings;
using Sequencer.Midi;
using Sequencer.View.Drawing;

namespace Sequencer.View.Control
{
    /// <summary>
    /// The beat ruler above the editor, with the playback position marker.
    /// </summary>
    public sealed class SequencerHeaderControl : UserControl
    {
        public static readonly StyledProperty<IPosition> CurrentPositionProperty =
            AvaloniaProperty.Register<SequencerHeaderControl, IPosition>(nameof(CurrentPosition));

        [NotNull] private readonly Canvas headerCanvas = new Canvas { Background = Brushes.Transparent };
        [CanBeNull] private PositionIndicatorHeaderDrawer positionIndicatorHeaderDrawer;
        [CanBeNull] private SequencerHeaderDrawer sequencerHeaderDrawer;

        public SequencerHeaderControl()
        {
            Content = headerCanvas;
            SizeChanged += (sender, args) => Redraw();
        }

        public IPosition CurrentPosition
        {
            get => GetValue(CurrentPositionProperty);
            set => SetValue(CurrentPositionProperty, value);
        }

        public void Initialise([NotNull] IPitchAndPositionCalculator pitchAndPositionCalculator, [NotNull] IDigitalAudioProtocol protocol,
            [NotNull] SequencerSettings sequencerSettings)
        {
            var canvasWrapper = new SequencerCanvasWrapper(headerCanvas);
            var dimensionsCalculator = new SequencerDimensionsCalculator(protocol, canvasWrapper, sequencerSettings, pitchAndPositionCalculator);
            sequencerHeaderDrawer = new SequencerHeaderDrawer(canvasWrapper, dimensionsCalculator, sequencerSettings);
            positionIndicatorHeaderDrawer = new PositionIndicatorHeaderDrawer(sequencerSettings, canvasWrapper, dimensionsCalculator);
            Redraw();
        }

        protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
        {
            base.OnPropertyChanged(change);

            if (change.Property == CurrentPositionProperty && change.NewValue is IPosition newPosition)
            {
                positionIndicatorHeaderDrawer?.DrawPositionIndicator(newPosition);
            }
        }

        /// <summary>
        /// The bar number at the left edge (1 for the first section, 5 for the second, and so on).
        /// </summary>
        public int FirstBarNumber
        {
            get => sequencerHeaderDrawer?.FirstBarNumber ?? 1;
            set
            {
                if (sequencerHeaderDrawer != null)
                {
                    sequencerHeaderDrawer.FirstBarNumber = value;
                    Redraw();
                }
            }
        }

        private void Redraw()
        {
            sequencerHeaderDrawer?.RedrawHeader();
            positionIndicatorHeaderDrawer?.RedrawEditor();
        }
    }
}
