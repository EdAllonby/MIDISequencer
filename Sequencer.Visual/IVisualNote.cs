using JetBrains.Annotations;
using Sequencer.Domain;

namespace Sequencer.Visual
{
    public interface IVisualNote : IPositionAware, ISequencedNote
    {
        NoteState NoteState { get; set; }

        [NotNull]
        Pitch Pitch { get; }

        [NotNull]
        IPosition StartPosition { [NotNull] get; [NotNull] set; }

        [NotNull]
        IPosition EndPosition { [NotNull] get; [NotNull] set; }

        [NotNull]
        Velocity Velocity { get; set; }

        void Draw();
        void MovePitchRelativeTo(int halfStepsToMove);

        void MovePositionRelativeTo(int ticksToMove);

        void Remove();
    }
}