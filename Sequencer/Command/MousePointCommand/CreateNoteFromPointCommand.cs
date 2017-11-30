using JetBrains.Annotations;
using Sequencer.Domain;
using Sequencer.Drawing;
using Sequencer.Input;
using Sequencer.View;

namespace Sequencer.Command.MousePointCommand
{
    /// <summary>
    /// Creates a <see cref="VisualNote" /> with correct pitch relative to the sequencer.
    /// </summary>
    public class CreateNoteFromPointCommand : MousePointNoteCommand
    {
        [NotNull] private readonly ISequencerCanvasWrapper sequencerCanvasWrapper;
        [NotNull] private readonly ISequencerNotes sequencerNotes;
        [NotNull] private readonly SequencerSettings sequencerSettings;
        [NotNull] private readonly IMouseOperator mouseOperator;
        [NotNull] private readonly ISequencerDimensionsCalculator sequencerDimensionsCalculator;

        public CreateNoteFromPointCommand([NotNull] ISequencerCanvasWrapper sequencerCanvasWrapper, [NotNull] ISequencerNotes sequencerNotes,
            [NotNull] SequencerSettings sequencerSettings, [NotNull] IMouseOperator mouseOperator,
            [NotNull] ISequencerDimensionsCalculator sequencerDimensionsCalculator)
        {
            this.sequencerCanvasWrapper = sequencerCanvasWrapper;
            this.sequencerNotes = sequencerNotes;
            this.sequencerSettings = sequencerSettings;
            this.mouseOperator = mouseOperator;
            this.sequencerDimensionsCalculator = sequencerDimensionsCalculator;
        }

        protected override bool CanExecute => mouseOperator.CanModifyNote;

        protected override void DoExecute(IMousePoint mousePoint)
        {
            sequencerNotes.MakeAllUnselected();
            Position notePosition = sequencerDimensionsCalculator.FindPositionFromPoint(mousePoint);
            Pitch pitch = sequencerDimensionsCalculator.FindPitchFromPoint(mousePoint);

            Position defaultEndPosition = GetDefaultEndPosition(notePosition);
            var defaultVelocity = new Velocity(64);

            var tone = new Tone(pitch, defaultVelocity, notePosition, defaultEndPosition);

            var newNote = new VisualNote(sequencerDimensionsCalculator, sequencerCanvasWrapper, sequencerSettings, tone);

            sequencerNotes.AddNote(newNote);
        }

        private Position GetDefaultEndPosition(Position notePosition)
        {
            return notePosition.NextPosition(sequencerSettings.TimeSignature);
        }
    }
}