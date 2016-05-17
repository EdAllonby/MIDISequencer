using System.Collections.Generic;
using Sequencer.View;

namespace Sequencer.Command.NotesCommand
{
    public interface INotesCommand
    {
        void Execute(IEnumerable<VisualNote> notes);
    }
}