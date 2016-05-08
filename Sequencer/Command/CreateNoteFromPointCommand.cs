using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
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

        protected override bool CanExecute()
        {
            return Mouse.RightButton == MouseButtonState.Pressed;
        }

        protected override void DoExecute(Point mousePoint)
        {
            sequencerNotes.ForEach(note => note.NoteState = NoteState.Unselected);
            Position notePosition = sequencerDimensionsCalculator.FindPositionFromPoint(mousePoint);
            Pitch pitch = sequencerDimensionsCalculator.FindPitchFromPoint(mousePoint);

            Position defaultEndPosition = GetDefaultEndPosition(notePosition);

            var newNote = new VisualNote(sequencerDimensionsCalculator, sequencerCanvas, sequencerSettings, notePosition, defaultEndPosition, pitch);
            newNote.Draw();
            sequencerNotes.Add(newNote);
        }

        private Position GetDefaultEndPosition(Position notePosition)
        {
            return notePosition.NextPosition(sequencerSettings.TimeSignature);
        }
    }
}