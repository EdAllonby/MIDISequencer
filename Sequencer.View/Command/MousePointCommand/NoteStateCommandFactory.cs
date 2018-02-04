using System;
using JetBrains.Annotations;
using Sequencer.View.Command.NotesCommand;
using Sequencer.Visual;
using Sequencer.Visual.Input;

namespace Sequencer.View.Command.MousePointCommand
{
    public class NoteStateCommandFactory : INoteStateCommandFactory
    {
        [NotNull] private readonly ISequencerNotes sequencerNotes;
        [NotNull] private readonly IKeyboardStateProcessor keyboardStateProcessor;

        public NoteStateCommandFactory([NotNull] ISequencerNotes sequencerNotes, [NotNull] IKeyboardStateProcessor keyboardStateProcessor)
        {
            this.sequencerNotes = sequencerNotes;
            this.keyboardStateProcessor = keyboardStateProcessor;
        }

        public INotesCommand CreateNoteStateCommand(NoteState noteState)
        {
            switch (noteState)
            {
                case NoteState.Selected:
                    return new UpdateNoteStateCommand(sequencerNotes, keyboardStateProcessor, NoteState.Selected);
                case NoteState.Unselected:
                    return new UpdateNoteStateCommand(sequencerNotes, keyboardStateProcessor, NoteState.Unselected);
                default:
                    throw new ArgumentOutOfRangeException(nameof(noteState), noteState, null);
            }
        }
    }
}