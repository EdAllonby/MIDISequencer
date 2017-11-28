using System.Collections.Generic;
using Sequencer.Domain;
using Sequencer.Drawing;
using Sequencer.View;
using Sequencer.ViewModel;

namespace Sequencer.Command.MousePointCommand
{
    public sealed class MousePointNoteCommandFactory
    {
        private readonly Dictionary<NoteAction, MousePointNoteCommand> noteCommandRegistry;

        public MousePointNoteCommandFactory(ISequencerCanvasWrapper sequencerCanvasWrapper, ISequencerNotes sequencerNotes,
            SequencerSettings sequencerSettings, SequencerDimensionsCalculator sequencerDimensionsCalculator)
        {
            noteCommandRegistry = new Dictionary<NoteAction, MousePointNoteCommand>
            {
                {NoteAction.Create, new CreateNoteFromPointCommand(sequencerCanvasWrapper, sequencerNotes, sequencerSettings, sequencerDimensionsCalculator)},
                {NoteAction.Select, new UpdateNoteStateFromPointCommand(sequencerNotes, sequencerSettings, sequencerDimensionsCalculator)},
                {NoteAction.Delete, new DeleteNoteFromPointCommand(sequencerNotes, sequencerSettings, sequencerDimensionsCalculator)}
            };
        }

        public MousePointNoteCommand FindCommand(NoteAction noteAction)
        {
            return noteCommandRegistry[noteAction];
        }
    }
}