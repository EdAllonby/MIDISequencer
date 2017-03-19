using System.Collections.Generic;
using Sequencer.Domain;
using Sequencer.View;

namespace Sequencer.Command.NotesCommand
{
    public sealed class ChangeVelocityCommand : INotesCommand
    {
        private readonly Velocity velocity;

        public ChangeVelocityCommand(Velocity velocity)
        {
            this.velocity = velocity;
        }

        public void Execute(IEnumerable<VisualNote> notes)
        {
            foreach (VisualNote visualNote in notes)
            {
                visualNote.Velocity = velocity;
            }
        }
    }
}