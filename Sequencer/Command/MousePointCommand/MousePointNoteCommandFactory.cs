﻿using System.Collections.Generic;
using JetBrains.Annotations;
using Sequencer.Domain;
using Sequencer.Drawing;
using Sequencer.Input;
using Sequencer.View;
using Sequencer.ViewModel;

namespace Sequencer.Command.MousePointCommand
{
    public sealed class MousePointNoteCommandFactory
    {
        private readonly Dictionary<NoteAction, MousePointNoteCommand> noteCommandRegistry;

        public MousePointNoteCommandFactory(ISequencerCanvasWrapper sequencerCanvasWrapper, [NotNull] IMouseOperator mouseOperator, IKeyboardStateProcessor keyboardStateProcessor, ISequencerNotes sequencerNotes,
            SequencerSettings sequencerSettings, ISequencerDimensionsCalculator sequencerDimensionsCalculator)
        {
            noteCommandRegistry = new Dictionary<NoteAction, MousePointNoteCommand>
            {
                {NoteAction.Create, new CreateNoteFromPointCommand(sequencerCanvasWrapper, sequencerNotes, sequencerSettings, mouseOperator, sequencerDimensionsCalculator)},
                {NoteAction.Select, new UpdateNoteStateFromPointCommand(sequencerNotes, mouseOperator, keyboardStateProcessor, sequencerSettings, sequencerDimensionsCalculator)},
                {NoteAction.Delete, new DeleteNoteFromPointCommand(sequencerNotes, mouseOperator, sequencerSettings, sequencerDimensionsCalculator)}
            };
        }

        public MousePointNoteCommand FindCommand(NoteAction noteAction)
        {
            return noteCommandRegistry[noteAction];
        }
    }
}