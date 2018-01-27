using System.Collections.Generic;
using JetBrains.Annotations;
using Sequencer.Domain.Settings;
using Sequencer.View.Command.NotesCommand;
using Sequencer.ViewModel;
using Sequencer.Visual;
using Sequencer.Visual.Input;

namespace Sequencer.View.Command.MousePointCommand
{
    public sealed class MousePointNoteCommandFactory
    {
        [NotNull] private readonly Dictionary<NoteAction, IMousePointNoteCommand> noteCommandRegistry;

        public MousePointNoteCommandFactory([NotNull] IVisualNoteFactory visualNoteFactory, [NotNull] IMouseOperator mouseOperator,
            [NotNull] IKeyboardStateProcessor keyboardStateProcessor, [NotNull] ISequencerNotes sequencerNotes,
            [NotNull] SequencerSettings sequencerSettings, [NotNull] ISequencerDimensionsCalculator sequencerDimensionsCalculator)
        {
            noteCommandRegistry = new Dictionary<NoteAction, IMousePointNoteCommand>
            {
                { NoteAction.Create, new CreateNoteFromPointCommand(visualNoteFactory, sequencerNotes, sequencerSettings, mouseOperator, sequencerDimensionsCalculator) },
                { NoteAction.Select, new UpdateNoteStateFromPointCommand(sequencerNotes, mouseOperator, keyboardStateProcessor, sequencerDimensionsCalculator) },
                { NoteAction.Delete, new DeleteNoteFromPointCommand(sequencerNotes, sequencerDimensionsCalculator, new DeleteNotesCommand(sequencerNotes)) }
            };
        }

        [NotNull]
        public IMousePointNoteCommand FindCommand([NotNull] NoteAction noteAction)
        {
            bool commandFound = noteCommandRegistry.TryGetValue(noteAction, out IMousePointNoteCommand matchingCommand);

            return commandFound ? matchingCommand : new EmptyMousePointCommand();
        }
    }
}