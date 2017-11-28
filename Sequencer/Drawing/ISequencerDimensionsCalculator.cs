using System.Windows;
using JetBrains.Annotations;
using Sequencer.Command.MousePointCommand;
using Sequencer.Domain;
using Sequencer.View;

namespace Sequencer.Drawing
{
    public interface ISequencerDimensionsCalculator
    {
        double BeatWidth { get; }

        double NoteHeight { get; }

        IVisualNote FindNoteFromPoint([NotNull] ISequencerNotes sequencerNotes, IMousePoint point);
        Pitch FindPitchFromPoint(IMousePoint mousePosition);
        Position FindPositionFromPoint(IMousePoint mousePosition);
    }
}