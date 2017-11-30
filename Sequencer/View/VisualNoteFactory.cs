using Sequencer.Domain;
using Sequencer.Drawing;

namespace Sequencer.View
{
    public class VisualNoteFactory : IVisualNoteFactory
    {
        private readonly SequencerSettings sequencerSettings;
        private readonly ISequencerDimensionsCalculator sequencerDimensionsCalculator;
        private readonly ISequencerCanvasWrapper sequencerCanvasWrapper;

        public VisualNoteFactory(SequencerSettings sequencerSettings, ISequencerDimensionsCalculator sequencerDimensionsCalculator, ISequencerCanvasWrapper sequencerCanvasWrapper)
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