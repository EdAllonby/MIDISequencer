using JetBrains.Annotations;
using Sequencer.Domain;
using Sequencer.Domain.Settings;
using Sequencer.Midi;

namespace Sequencer.Visual
{
    public class VisualNoteFactory : IVisualNoteFactory
    {
        [NotNull] private readonly IDigitalAudioProtocol protocol;
        [NotNull] private readonly ISequencerCanvasWrapper sequencerCanvasWrapper;
        [NotNull] private readonly ISequencerDimensionsCalculator sequencerDimensionsCalculator;
        [NotNull] private readonly SequencerSettings sequencerSettings;

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