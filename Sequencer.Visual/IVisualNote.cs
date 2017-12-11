using JetBrains.Annotations;
using Sequencer.Domain;

namespace Sequencer.Visual
{
    public interface IVisualNote : IPositionAware
    {
        NoteState NoteState { get; set; }

        [NotNull]
        Pitch Pitch { get; }

        [NotNull]
        IPosition StartPosition { [NotNull] get; [NotNull] set; }

        [NotNull]
        IPosition EndPosition { [NotNull] get; [NotNull] set; }


        [NotNull]
        Tone Tone { get; }

        [NotNull]
        Velocity Velocity { get; set; }

        void Draw();
        void MovePitchRelativeTo(int halfStepsToMove);
        void MovePositionRelativeTo(int beatsToMove);
        void Remove();
    }
}