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
        private readonly ISequencerCanvasWrapper sequencerCanvasWrapper;

        public CreateNoteFromPointCommand([NotNull] ISequencerCanvasWrapper sequencerCanvasWrapper, [NotNull] ISequencerNotes sequencerNotes,
            [NotNull] SequencerSettings sequencerSettings, [NotNull] IMouseOperator mouseOperator,
            [NotNull] ISequencerDimensionsCalculator sequencerDimensionsCalculator)
            : base(sequencerNotes, mouseOperator, sequencerSettings, sequencerDimensionsCalculator)
        {
            this.sequencerCanvasWrapper = sequencerCanvasWrapper;
        }

        protected override bool CanExecute => MouseOperator.CanModifyNote;

        protected override void DoExecute(IMousePoint mousePoint)
        {
            SequencerNotes.MakeAllUnselected();
            Position notePosition = SequencerDimensionsCalculator.FindPositionFromPoint(mousePoint);
            Pitch pitch = SequencerDimensionsCalculator.FindPitchFromPoint(mousePoint);

            Position defaultEndPosition = GetDefaultEndPosition(notePosition);
            var defaultVelocity = new Velocity(64);

            var  tone = new Tone(pitch, defaultVelocity, notePosition, defaultEndPosition);

            var newNote = new VisualNote(SequencerDimensionsCalculator, sequencerCanvasWrapper, SequencerSettings, tone);

            SequencerNotes.AddNote(newNote);
        }

        private Position GetDefaultEndPosition(Position notePosition)
        {
            return notePosition.NextPosition(SequencerSettings.TimeSignature);
        }
    }
}