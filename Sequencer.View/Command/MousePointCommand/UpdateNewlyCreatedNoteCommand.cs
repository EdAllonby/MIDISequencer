using JetBrains.Annotations;
using Sequencer.Domain;
using Sequencer.Domain.Settings;
using Sequencer.Visual;
using Sequencer.Visual.Input;

namespace Sequencer.View.Command.MousePointCommand
{
    /// <summary>
    /// Command to update a <see cref="VisualNote" />'s end position based on the current mouse position in the sequencer.
    /// </summary>
    public sealed class UpdateNewlyCreatedNoteCommand : MousePointNoteCommand
    {
        [NotNull] private readonly IMouseOperator mouseOperator;
        [NotNull] private readonly IMusicalSettings musicalSettings;
        [NotNull] private readonly ISequencerDimensionsCalculator sequencerDimensionsCalculator;
        [NotNull] private readonly ISequencerNotes sequencerNotes;

        public UpdateNewlyCreatedNoteCommand([NotNull] ISequencerNotes sequencerNotes, [NotNull] IMouseOperator mouseOperator,
            [NotNull] IMusicalSettings musicalSettings, [NotNull] ISequencerDimensionsCalculator sequencerDimensionsCalculator)
        {
            this.sequencerNotes = sequencerNotes;
            this.mouseOperator = mouseOperator;
            this.musicalSettings = musicalSettings;
            this.sequencerDimensionsCalculator = sequencerDimensionsCalculator;
        }

        protected override bool CanExecute => mouseOperator.CanModifyNote;

        protected override void DoExecute(IMousePoint mousePosition)
        {
            IPosition currentEndPosition = sequencerDimensionsCalculator.FindPositionFromPoint(mousePosition);
            IPosition nextPosition = currentEndPosition.NextPosition(musicalSettings.NoteResolution, musicalSettings.TimeSignature, musicalSettings.TicksPerQuarterNote);

            foreach (IVisualNote selectedNote in sequencerNotes.SelectedNotes)
            {
                selectedNote.EndPosition = nextPosition;
            }
        }
    }
}