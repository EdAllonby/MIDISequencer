using System.Collections.Generic;
using Sequencer.View;

namespace Sequencer.Command.NotesCommand
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