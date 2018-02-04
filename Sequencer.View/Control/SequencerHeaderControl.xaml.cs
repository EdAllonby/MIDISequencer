using System;
using System.Windows;
using JetBrains.Annotations;
using Microsoft.Practices.ServiceLocation;
using Sequencer.Domain;
using Sequencer.Domain.Settings;
using Sequencer.Midi;
using Sequencer.View.Drawing;

namespace Sequencer.View.Control
{
    public partial class SequencerHeaderControl
    {
        [NotNull] public static readonly DependencyProperty CurrentPositionProperty =
            DependencyProperty.Register(nameof(CurrentPosition), typeof(IPosition), typeof(SequencerHeaderControl),
                new FrameworkPropertyMetadata(OnCurrentPositionChanged));

        [NotNull] private readonly PositionIndicatorHeaderDrawer positionIndicatorHeaderDrawer;
        [NotNull] private readonly SequencerHeaderDrawer sequencerHeaderDrawer;
        [NotNull] private readonly SequencerSettings sequencerSettings = new SequencerSettings();

        public SequencerHeaderControl()
        {
            InitializeComponent();

            SizeChanged += SequencerSizeChanged;

            // ReSharper disable PossibleNullReferenceException
            var pitchAndPositionCalculator = ServiceLocator.Current.GetInstance<IPitchAndPositionCalculator>();
            // ReSharper restore PossibleNullReferenceException

            IDigitalAudioProtocol protocol = new MidiProtocol(pitchAndPositionCalculator);
            var sequencerHeaderCanvasWrapper = new SequencerCanvasWrapper(SequencerHeaderCanvas);

            var sequencerDimensionsCalculator = new SequencerDimensionsCalculator(protocol, sequencerHeaderCanvasWrapper, sequencerSettings, pitchAndPositionCalculator);

            sequencerHeaderDrawer = new SequencerHeaderDrawer(sequencerHeaderCanvasWrapper, sequencerDimensionsCalculator, sequencerSettings);

            positionIndicatorHeaderDrawer = new PositionIndicatorHeaderDrawer(sequencerSettings, sequencerHeaderCanvasWrapper, sequencerDimensionsCalculator);
        }

        [NotNull]
        public IPosition CurrentPosition
        {
            get => (IPosition) GetValue(CurrentPositionProperty) ?? throw new InvalidOperationException();
            set => SetValue(CurrentPositionProperty, value);
        }

        private void SequencerSizeChanged(object sender, RoutedEventArgs e)
        {
            sequencerHeaderDrawer.RedrawHeader();
            positionIndicatorHeaderDrawer.RedrawEditor();
        }

        private static void OnCurrentPositionChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is SequencerHeaderControl component)
            {
                PositionIndicatorHeaderDrawer indicatorDrawer = component.positionIndicatorHeaderDrawer;
                if (e.NewValue is IPosition newPosition)
                {
                    indicatorDrawer.DrawPositionIndicator(newPosition);
                }
            }
        }
    }
}