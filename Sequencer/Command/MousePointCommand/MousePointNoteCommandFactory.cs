﻿using System.Collections.Generic;
using System.Windows.Controls;
using Sequencer.Domain;
using Sequencer.Drawing;
using Sequencer.View;
using Sequencer.ViewModel;

namespace Sequencer.Command.MousePointCommand
{
    public sealed class MousePointNoteCommandFactory
    {
        private readonly Dictionary<NoteAction, MousePointNoteCommand> noteCommandRegistry;

        public MousePointNoteCommandFactory(Canvas sequencerCanvas, SequencerNotes sequencerNotes,
            SequencerSettings sequencerSettings, SequencerDimensionsCalculator sequencerDimensionsCalculator)
        {
            noteCommandRegistry = new Dictionary<NoteAction, MousePointNoteCommand>
            {
                {NoteAction.Create, new CreateNoteFromPointCommand(sequencerCanvas, sequencerNotes, sequencerSettings, sequencerDimensionsCalculator)},
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