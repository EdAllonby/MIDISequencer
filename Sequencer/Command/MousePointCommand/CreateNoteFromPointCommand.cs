using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using JetBrains.Annotations;
using Sequencer.Domain;

namespace Sequencer.Command.MousePointCommand
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

        protected override bool CanExecute()
        {
            return Mouse.RightButton == MouseButtonState.Pressed;
        }

        protected override void DoExecute(Point mousePoint)
        {
            SequencerNotes.ForEach(note => note.NoteState = NoteState.Unselected);
            Position notePosition = SequencerDimensionsCalculator.FindPositionFromPoint(mousePoint);
            Pitch pitch = SequencerDimensionsCalculator.FindPitchFromPoint(mousePoint);

            Position defaultEndPosition = GetDefaultEndPosition(notePosition);

            var newNote = new VisualNote(SequencerDimensionsCalculator, sequencerCanvas, SequencerSettings, notePosition, defaultEndPosition, pitch);
            newNote.Draw();
            SequencerNotes.Add(newNote);
        }

        private Position GetDefaultEndPosition(Position notePosition)
        {
            return notePosition.NextPosition(SequencerSettings.TimeSignature);
        }
    }
}