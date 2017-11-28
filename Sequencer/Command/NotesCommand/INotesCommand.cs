using System.Collections.Generic;
using JetBrains.Annotations;
using Sequencer.View;

namespace Sequencer.Command.NotesCommand
{
    public interface INotesCommand
    {
        void Execute([NotNull] IEnumerable<IVisualNote> notes);
    }
}