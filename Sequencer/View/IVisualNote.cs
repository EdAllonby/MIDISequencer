using System.Windows.Media;
using Sequencer.Domain;

namespace Sequencer.View
{
    public interface IVisualNote : IPositionAware
    {
        Position EndPosition { get; set; }
        NoteState NoteState { get; set; }
        Pitch Pitch { get; }
        Position StartPosition { get; set; }
        Tone Tone { get; }
        Velocity Velocity { get; set; }

        void Draw();
        bool IntersectsWith(Geometry geometry);
        void MovePitchRelativeTo(int halfStepsToMove);
        void MovePositionRelativeTo(int beatsToMove);
        void Remove();
        string ToString();
    }
}