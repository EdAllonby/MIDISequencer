using System.Collections.Generic;
using JetBrains.Annotations;
using Sequencer.Domain;
using Sequencer.View.Control;

namespace Sequencer.View.Command.NotesCommand
{
    public sealed class ChangeVelocityCommand : INotesCommand
    {
        [NotNull] private readonly Velocity velocity;

        public ChangeVelocityCommand([NotNull] Velocity velocity)
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