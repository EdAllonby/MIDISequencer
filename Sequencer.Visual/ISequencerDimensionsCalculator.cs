using JetBrains.Annotations;
using Sequencer.Domain;
using Sequencer.Visual.Input;

namespace Sequencer.Visual
{
    public interface ISequencerDimensionsCalculator
    {
        double BeatWidth { get; }

        double NoteHeight { get; }

        double MeasureWidth { get; }

        double BarWidth { get; }

        double SixteenthNoteWidth { get; }

        [CanBeNull]
        IVisualNote FindNoteFromPoint([NotNull] ISequencerNotes sequencerNotes, [NotNull] IMousePoint point);

        [NotNull]
        Pitch FindPitchFromPoint([NotNull] IMousePoint mousePosition);

        [NotNull]
        IPosition FindPositionFromPoint([NotNull] IMousePoint mousePosition);

        bool IsPointInsideNote([NotNull] ISequencerNotes notes, [NotNull] IMousePoint mouseDownPoint);

        /// <summary>
        /// Whether the point is on the note's start edge, end edge, or neither.
        /// </summary>
        NoteEdge EdgeAtPoint([NotNull] IVisualNote note, [NotNull] IMousePoint point);

        double GetPointFromPosition([NotNull] IPosition position);

        double GetPointFromPitch([NotNull] Pitch pitch);
    }
}