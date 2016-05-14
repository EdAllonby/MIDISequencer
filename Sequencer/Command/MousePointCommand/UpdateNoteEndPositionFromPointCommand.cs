using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using JetBrains.Annotations;
using Sequencer.Domain;

namespace Sequencer.Command.MousePointCommand
{
    /// <summary>
    /// Command to update a <see cref="VisualNote" />'s end position based on the current mouse position in the sequencer.
    /// </summary>
    public sealed class UpdateNoteEndPositionFromPointCommand : MousePointNoteCommand
    {
        public UpdateNoteEndPositionFromPointCommand([NotNull] List<VisualNote> sequencerNotes,
            [NotNull] SequencerSettings sequencerSettings, [NotNull] SequencerDimensionsCalculator sequencerDimensionsCalculator)
            : base(sequencerNotes, sequencerSettings, sequencerDimensionsCalculator)
        {
        }

        protected override bool CanExecute()
        {
            return (Mouse.LeftButton == MouseButtonState.Pressed) || (Mouse.RightButton == MouseButtonState.Pressed);
        }

        protected override void DoExecute(Point mousePosition)
        {
            var selectedNotes = SequencerNotes.Where(note => note.NoteState == NoteState.Selected);

            Position currentEndPosition = SequencerDimensionsCalculator.FindPositionFromPoint(mousePosition);
            Position nextPosition = currentEndPosition.NextPosition(SequencerSettings.TimeSignature);

            VisualNote noteToUpdate = selectedNotes.FirstOrDefault();

            if (noteToUpdate != null)
            {
                noteToUpdate.EndPosition = nextPosition;
            }
        }
    }
}