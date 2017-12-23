using System.Collections.Generic;
using Sequencer.Visual;

namespace Sequencer.View.Command.NotesCommand
{
    public sealed class MoveNotePositionCommand : INotesCommand
    {
        private readonly int ticksToMove;

        public MoveNotePositionCommand(int ticksToMove)
        {
            this.ticksToMove = ticksToMove;
        }

        public void Execute(IEnumerable<IVisualNote> notes)
        {
            foreach (IVisualNote visualNote in notes)
            {
                visualNote.MovePositionRelativeTo(ticksToMove);
            }
        }
    }
}