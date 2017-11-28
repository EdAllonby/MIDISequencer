using Sequencer.Domain;

namespace Sequencer.View
{
    public interface IVisualNote : IPositionAware
    {
        Position EndPosition { get; }
        NoteState NoteState { get; set; }
        Pitch Pitch { get; }
        Position StartPosition { get; }
        Tone Tone { get; }
        Velocity Velocity { get; set; }
        void Draw();
        void MovePitchRelativeTo(int halfStepsToMove);
        void MovePositionRelativeTo(int beatsToMove);
        void Remove();
    }
}