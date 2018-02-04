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
        }

        private void SequencerSizeChanged(object sender, RoutedEventArgs e)
        {
            sequencerHeaderDrawer.RedrawHeader();
        }
    }
}