using System.Collections.Generic;

namespace Sequencer.Command
{
    public sealed class MoveNotePitchCommand : INotesCommand
    {
        private readonly int pitchesToMove;

        public MoveNotePitchCommand(int pitchesToMove)
        {
            this.pitchesToMove = pitchesToMove;
        }

        public void Execute(IEnumerable<VisualNote> notes)
        {
            foreach (VisualNote visualNote in notes)
            {
                visualNote.MovePitchRelativeTo(pitchesToMove);
            }
        }
    }
}