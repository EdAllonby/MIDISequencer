using System.Collections.Generic;

namespace Sequencer.Command
{
    public interface INotesCommand
    {
        void Execute(IEnumerable<VisualNote> notes);
    }
}
