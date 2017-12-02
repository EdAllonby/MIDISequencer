using System.Collections.Generic;
using Sequencer.Domain;
using Sequencer.Utilities;
using Sequencer.View.Control;

namespace Sequencer.View.Command.NotesCommand
{
    public sealed class DecrementVelocityCommand : INotesCommand
    {
        private readonly int velocityIncrement;

        public DecrementVelocityCommand(int velocityIncrement)
        {
            this.velocityIncrement = velocityIncrement;
        }

        public void Execute(IEnumerable<IVisualNote> notes)
        {
            foreach (IVisualNote visualNote in notes)
            {
                var currentVelocity = visualNote.Velocity;

                Velocity newVelocity = currentVelocity - velocityIncrement;
                var incrementCommand = new ChangeVelocityCommand(newVelocity);
                incrementCommand.Execute(visualNote.Yield());
            }
        }
    }
}