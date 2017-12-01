using JetBrains.Annotations;
using Sequencer.Domain;
using Sequencer.Drawing;

namespace Sequencer.View
{
    public class VisualNoteFactory : IVisualNoteFactory
    {
        [NotNull] private readonly SequencerSettings sequencerSettings;
        [NotNull] private readonly ISequencerDimensionsCalculator sequencerDimensionsCalculator;
        [NotNull] private readonly ISequencerCanvasWrapper sequencerCanvasWrapper;

        public VisualNoteFactory([NotNull] SequencerSettings sequencerSettings,
            [NotNull] ISequencerDimensionsCalculator sequencerDimensionsCalculator, [NotNull] ISequencerCanvasWrapper sequencerCanvasWrapper)
        {
            this.sequencerSettings = sequencerSettings;
            this.sequencerDimensionsCalculator = sequencerDimensionsCalculator;
            this.sequencerCanvasWrapper = sequencerCanvasWrapper;
        }

        public IVisualNote CreateNote(Pitch pitch, IPosition start, IPosition end)
        {
            var tone = new Tone(pitch, sequencerSettings.DefaultVelocity, start, end);

            return new VisualNote(sequencerDimensionsCalculator, sequencerCanvasWrapper, sequencerSettings, tone);
        }
    }
}