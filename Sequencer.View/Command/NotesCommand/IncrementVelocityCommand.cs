using System.Collections.Generic;
using Sequencer.Domain;
using Sequencer.Utilities;
using Sequencer.Visual;

namespace Sequencer.View.Command.NotesCommand
{
    public sealed class IncrementVelocityCommand : INotesCommand
    {
        private readonly int velocityIncrement;

        public IncrementVelocityCommand(int velocityIncrement)
        {
            this.velocityIncrement = velocityIncrement;
        }

        public void Execute(IEnumerable<IVisualNote> notes)
        {
            foreach (IVisualNote visualNote in notes)
            {
                Velocity currentVelocity = visualNote.Velocity;

                Velocity newVelocity = currentVelocity + velocityIncrement;
                var incrementCommand = new ChangeVelocityCommand(newVelocity);
                incrementCommand.Execute(visualNote.Yield());
            }
        }
    }
}