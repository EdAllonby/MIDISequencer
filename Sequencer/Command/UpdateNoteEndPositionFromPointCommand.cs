using System.Collections.Generic;
using System.Linq;
using System.Windows;
using JetBrains.Annotations;
using Sequencer.Domain;

namespace Sequencer.Command
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

        public override void Execute(Point mousePosition)
        {
            var selectedNotes = sequencerNotes.Where(note => note.NoteState == NoteState.Selected);

            Position currentEndPosition = sequencerDimensionsCalculator.FindNotePositionFromPoint(mousePosition);
            Position nextPosition = currentEndPosition.NextPosition(sequencerSettings.TimeSignature);

            selectedNotes.FirstOrDefault()?.UpdateNoteLength(sequencerSettings.TimeSignature, nextPosition, sequencerDimensionsCalculator.BeatWidth);
        }
    }
}