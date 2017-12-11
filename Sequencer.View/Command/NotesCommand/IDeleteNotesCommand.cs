using System.Collections.Generic;
using JetBrains.Annotations;
using Sequencer.Visual;

namespace Sequencer.View.Command.NotesCommand
{
    public interface IDeleteNotesCommand
    {
        void Execute([NotNull] [ItemNotNull] IEnumerable<IVisualNote> notesToDelete);
    }
}