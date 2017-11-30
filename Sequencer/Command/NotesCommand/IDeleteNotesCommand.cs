using System.Collections.Generic;
using Sequencer.View;

namespace Sequencer.Command.NotesCommand
{
    public interface IDeleteNotesCommand
    {
        void Execute(IEnumerable<IVisualNote> notesToDelete);
    }
}