using JetBrains.Annotations;
using Sequencer.Domain;
using Sequencer.Input;
using Sequencer.View;

namespace Sequencer.Drawing
{
    public interface ISequencerDimensionsCalculator
    {
        double BeatWidth { get; }

        double NoteHeight { get; }

        double MeasureWidth { get; }
        double BarWidth { get; }

        [CanBeNull]
        IVisualNote FindNoteFromPoint([NotNull] ISequencerNotes sequencerNotes, [NotNull] IMousePoint point);

        [NotNull]
        Pitch FindPitchFromPoint([NotNull] IMousePoint mousePosition);

        [NotNull]
        IPosition FindPositionFromPoint([NotNull] IMousePoint mousePosition);

        bool IsPointInsideNote([NotNull] ISequencerNotes notes, [NotNull] IMousePoint mouseDownPoint);

        [CanBeNull]
        IVisualNote NoteAtEndingPoint([NotNull] ISequencerNotes notes, [NotNull] IMousePoint mouseDownPoint);

        [CanBeNull]
        IVisualNote NoteAtStartingPoint([NotNull] ISequencerNotes notes, [NotNull] IMousePoint mouseDownPoint);
    }
}