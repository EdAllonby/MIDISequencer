using JetBrains.Annotations;
using Sequencer.View.Command.NotesCommand;
using Sequencer.Visual;

namespace Sequencer.View.Command.MousePointCommand
{
    public interface INoteStateCommandFactory
    {
        [NotNull]
        INotesCommand CreateNoteStateCommand(NoteState noteState);
    }
}