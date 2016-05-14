using System.Collections.Generic;
using System.Windows.Controls;
using Sequencer.Domain;

namespace Sequencer.Command.MousePointCommand
{
    public sealed class MousePointNoteCommandFactory
    {
        private readonly Dictionary<NoteAction, MousePointNoteCommand> noteCommandRegistry;

        public MousePointNoteCommandFactory(Canvas sequencerCanvas, List<VisualNote> sequencerNotes,
            SequencerSettings sequencerSettings, SequencerDimensionsCalculator sequencerDimensionsCalculator)
        {
            noteCommandRegistry = new Dictionary<NoteAction, MousePointNoteCommand>
            {
                {NoteAction.Create, new CreateNoteFromPointCommand(sequencerCanvas, sequencerNotes, sequencerSettings, sequencerDimensionsCalculator)},
                {NoteAction.Select, new UpdateNoteStateFromPointCommand(sequencerNotes, sequencerSettings, sequencerDimensionsCalculator)},
                {NoteAction.Delete, new DeleteNoteFromPointCommand(sequencerCanvas, sequencerNotes, sequencerSettings, sequencerDimensionsCalculator)}
            };
        }

        public MousePointNoteCommand FindCommand(NoteAction noteAction)
        {
            return noteCommandRegistry[noteAction];
        }
    }
}