using System.Windows;using System.Windows.Controls;using JetBrains.Annotations;using Sequencer.Domain;using Sequencer.Drawing;using Sequencer.Input;using Sequencer.View;

namespace Sequencer.Command.MousePointCommand
{
    /// <summary>
    /// Creates a <see cref="VisualNote" /> with correct pitch relative to the sequencer.
    /// </summary>
    public class CreateNoteFromPointCommand : MousePointNoteCommand
    {
        private readonly Canvas sequencerCanvas;

        public CreateNoteFromPointCommand([NotNull] Canvas sequencerCanvas, [NotNull] SequencerNotes sequencerNotes,
            [NotNull] SequencerSettings sequencerSettings,
            [NotNull] SequencerDimensionsCalculator sequencerDimensionsCalculator)
            : base(sequencerNotes, sequencerSettings, sequencerDimensionsCalculator)
        {
            this.sequencerCanvas = sequencerCanvas;
        }

        protected override bool CanExecute => MouseOperator.CanModifyNote;

        protected override void DoExecute(Point mousePoint)
        {
            SequencerNotes.MakeAllUnselected();
            Position notePosition = SequencerDimensionsCalculator.FindPositionFromPoint(mousePoint);
            Pitch pitch = SequencerDimensionsCalculator.FindPitchFromPoint(mousePoint);

            Position defaultEndPosition = GetDefaultEndPosition(notePosition);
            var defaultVelocity = new Velocity(64);

            var  tone = new Tone(pitch, defaultVelocity, notePosition, defaultEndPosition);

            var newNote = new VisualNote(SequencerDimensionsCalculator, sequencerCanvas, SequencerSettings, tone);

            SequencerNotes.AddNote(newNote);
        }

        private Position GetDefaultEndPosition(Position notePosition)
        {
            return notePosition.NextPosition(SequencerSettings.TimeSignature);
        }
    }
}