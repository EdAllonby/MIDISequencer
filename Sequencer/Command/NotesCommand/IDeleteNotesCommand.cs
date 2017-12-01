using System.Collections.Generic;
using JetBrains.Annotations;
using Sequencer.View;

namespace Sequencer.Command.NotesCommand
{
    public interface IDeleteNotesCommand
    {
        void Execute([NotNull] [ItemNotNull] IEnumerable<IVisualNote> notesToDelete);
    }
}