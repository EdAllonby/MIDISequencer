﻿using System.Collections.Generic;
using System.Windows.Controls;

namespace Sequencer.Command
{
    public sealed class NoteCommandFactory
    {
        private readonly Dictionary<NoteAction, NoteCommand> noteCommandRegistry;

        public NoteCommandFactory(Canvas sequencerCanvas, List<VisualNote> sequencerNotes,
            SequencerSettings sequencerSettings, SequencerDimensionsCalculator sequencerDimensionsCalculator)
        {
            noteCommandRegistry = new Dictionary<NoteAction, NoteCommand>
            {
                {NoteAction.Create, new CreateNoteCommand(sequencerCanvas, sequencerNotes, sequencerSettings, sequencerDimensionsCalculator)},
                {NoteAction.Select, new UpdateNoteStateCommand(sequencerNotes, sequencerSettings, sequencerDimensionsCalculator)},
                {NoteAction.Delete, new DeleteNoteCommand(sequencerCanvas, sequencerNotes, sequencerSettings, sequencerDimensionsCalculator)}
            };
        }

        public NoteCommand FindCommand(NoteAction noteAction)
        {
            return noteCommandRegistry[noteAction];
        }
    }
}