using System.Collections.Generic;
using Sequencer.View;

namespace Sequencer.Command.NotesCommand
{
    public sealed class MoveNotePitchCommand : INotesCommand
    {
        private readonly int halfStepsToMove;

        public MoveNotePitchCommand(int halfStepsToMove)
        {
            this.halfStepsToMove = halfStepsToMove;
        }

        public void Execute(IEnumerable<VisualNote> notes)
        {
            foreach (VisualNote visualNote in notes)
            {
                visualNote.MovePitchRelativeTo(halfStepsToMove);
            }
        }
    }
}