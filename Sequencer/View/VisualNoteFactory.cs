using JetBrains.Annotations;
using Sequencer.Domain;
using Sequencer.Drawing;
using Sequencer.Shared;

namespace Sequencer.View
{
    public class VisualNoteFactory : IVisualNoteFactory
    {
        [NotNull] private readonly IPitchAndPositionCalculator pitchAndPositionCalculator;
        [NotNull] private readonly SequencerSettings sequencerSettings;
        [NotNull] private readonly ISequencerDimensionsCalculator sequencerDimensionsCalculator;
        [NotNull] private readonly ISequencerCanvasWrapper sequencerCanvasWrapper;

        public VisualNoteFactory([NotNull] IPitchAndPositionCalculator pitchAndPositionCalculator, [NotNull] SequencerSettings sequencerSettings,
            [NotNull] ISequencerDimensionsCalculator sequencerDimensionsCalculator, [NotNull] ISequencerCanvasWrapper sequencerCanvasWrapper)
        {
            this.pitchAndPositionCalculator = pitchAndPositionCalculator;
            this.sequencerSettings = sequencerSettings;
            this.sequencerDimensionsCalculator = sequencerDimensionsCalculator;
            this.sequencerCanvasWrapper = sequencerCanvasWrapper;
        }

        public IVisualNote CreateNote(Pitch pitch, IPosition start, IPosition end)
        {
            var tone = new Tone(pitch, sequencerSettings.DefaultVelocity, start, end);

            return new VisualNote(pitchAndPositionCalculator, sequencerDimensionsCalculator, sequencerCanvasWrapper, sequencerSettings, tone);
        }
    }
}