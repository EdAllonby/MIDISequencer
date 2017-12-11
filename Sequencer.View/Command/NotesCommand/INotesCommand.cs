using System.Collections.Generic;
using JetBrains.Annotations;
using Sequencer.Visual;

namespace Sequencer.View.Command.NotesCommand
{
    public interface INotesCommand
    {
        void Execute([NotNull] [ItemNotNull] IEnumerable<IVisualNote> notes);
    }
}