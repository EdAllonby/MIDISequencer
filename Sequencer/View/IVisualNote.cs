using Sequencer.Domain;

namespace Sequencer.View
{
    public interface IVisualNote : IPositionAware
    {
        IPosition EndPosition { get; }
        NoteState NoteState { get; set; }
        Pitch Pitch { get; }
        IPosition StartPosition { get; }
        Tone Tone { get; }
        Velocity Velocity { get; set; }
        void Draw();
        void MovePitchRelativeTo(int halfStepsToMove);
        void MovePositionRelativeTo(int beatsToMove);
        void Remove();
    }
}