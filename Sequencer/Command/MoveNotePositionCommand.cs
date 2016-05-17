using System.Collections.Generic;

namespace Sequencer.Command
{
    public sealed class MoveNotePositionCommand : INotesCommand
    {
        private readonly int beatsToMove;

        public MoveNotePositionCommand(int beatsToMove)
        {
            this.beatsToMove = beatsToMove;
        }

        public void Execute(IEnumerable<VisualNote> notes)
        {
            foreach (VisualNote visualNote in notes)
            {
                visualNote.MovePositionRelativeTo(beatsToMove);
            }
        }
    }
}