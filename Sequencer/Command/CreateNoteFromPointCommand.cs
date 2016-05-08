﻿using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using JetBrains.Annotations;
using Sequencer.Domain;

namespace Sequencer.Command
{
    /// <summary>
    /// Creates a <see cref="VisualNote" /> with correct pitch relative to the sequencer.
    /// </summary>
    public class CreateNoteFromPointCommand : MousePointNoteCommand
    {
        private readonly Canvas sequencerCanvas;

        public CreateNoteFromPointCommand([NotNull] Canvas sequencerCanvas, [NotNull] List<VisualNote> sequencerNotes,
            [NotNull] SequencerSettings sequencerSettings, [NotNull] SequencerDimensionsCalculator sequencerDimensionsCalculator)
            : base(sequencerNotes, sequencerSettings, sequencerDimensionsCalculator)
        {
            this.sequencerCanvas = sequencerCanvas;
        }

        public override void Execute(Point mousePoint)
        {
            sequencerNotes.ForEach(note => note.NoteState = NoteState.Unselected);
            Position notePosition = sequencerDimensionsCalculator.FindNotePositionFromPoint(mousePoint);
            Pitch pitch = sequencerDimensionsCalculator.FindPitch(mousePoint);

            Position defaultEndPosition = GetDefaultEndPosition(notePosition);

            var newNote = new VisualNote(sequencerSettings, notePosition, defaultEndPosition, pitch);
            newNote.Draw(sequencerDimensionsCalculator, sequencerCanvas);
            sequencerNotes.Add(newNote);
        }

        private Position GetDefaultEndPosition(Position notePosition)
        {
            return notePosition.NextPosition(sequencerSettings.TimeSignature);
        }
    }
}