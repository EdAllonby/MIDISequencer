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

        public void Execute(IEnumerable<IVisualNote> notes)
        {
            foreach (IVisualNote visualNote in notes)
            {
                visualNote.Velocity = velocity;
            }
        }
    }
}