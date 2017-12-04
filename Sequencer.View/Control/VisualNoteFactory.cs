using JetBrains.Annotations;
using Sequencer.Domain;
using Sequencer.Midi;
using Sequencer.Shared;
using Sequencer.View.Drawing;

namespace Sequencer.View.Control
{
    public class VisualNoteFactory : IVisualNoteFactory
    {
        [NotNull] private readonly ISequencerCanvasWrapper sequencerCanvasWrapper;
        [NotNull] private readonly ISequencerDimensionsCalculator sequencerDimensionsCalculator;
        [NotNull] private readonly SequencerSettings sequencerSettings;
        [NotNull] private readonly IDigitalAudioProtocol protocol;

        public VisualNoteFactory([NotNull] SequencerSettings sequencerSettings, [NotNull] IDigitalAudioProtocol protocol,
            [NotNull] ISequencerDimensionsCalculator sequencerDimensionsCalculator, [NotNull] ISequencerCanvasWrapper sequencerCanvasWrapper)
        {
            this.sequencerSettings = sequencerSettings;
            this.protocol = protocol;
            this.sequencerDimensionsCalculator = sequencerDimensionsCalculator;
            this.sequencerCanvasWrapper = sequencerCanvasWrapper;
        }

        public IVisualNote CreateNote(Pitch pitch, IPosition start, IPosition end)
        {
            var tone = new Tone(pitch, sequencerSettings.DefaultVelocity, start, end);

            return new VisualNote(protocol, sequencerDimensionsCalculator, sequencerCanvasWrapper, sequencerSettings, tone);
        }
    }
}