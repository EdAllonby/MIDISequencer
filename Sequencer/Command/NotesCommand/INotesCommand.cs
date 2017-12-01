using System.Collections.Generic;
using JetBrains.Annotations;
using Sequencer.View;

namespace Sequencer.Command.NotesCommand
{
    public interface INotesCommand
    {
        void Execute([NotNull] [ItemNotNull] IEnumerable<IVisualNote> notes);
    }
}