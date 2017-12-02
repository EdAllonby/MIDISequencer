using System.Collections.Generic;
using Sequencer.View.Control;

namespace Sequencer.View.Command.NotesCommand
{
    public sealed class MoveNotePitchCommand : INotesCommand
    {
        private readonly int halfStepsToMove;

        public MoveNotePitchCommand(int halfStepsToMove)
        {
            this.halfStepsToMove = halfStepsToMove;
        }

        public void Execute(IEnumerable<IVisualNote> notes)
        {
            foreach (IVisualNote visualNote in notes)
            {
                visualNote.MovePitchRelativeTo(halfStepsToMove);
            }
        }
    }
}