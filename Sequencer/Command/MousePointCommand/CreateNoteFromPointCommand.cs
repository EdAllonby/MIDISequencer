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

        public CreateNoteFromPointCommand([NotNull] Canvas sequencerCanvas, [NotNull] SequencerNotes sequencerNotes,
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
            SequencerNotes.MakeAllUnselected();
            Position notePosition = SequencerDimensionsCalculator.FindPositionFromPoint(mousePoint);
            Pitch pitch = SequencerDimensionsCalculator.FindPitchFromPoint(mousePoint);

            Position defaultEndPosition = GetDefaultEndPosition(notePosition);

            var newNote = new VisualNote(SequencerDimensionsCalculator, sequencerCanvas, SequencerSettings, 
                new Velocity(64), notePosition, defaultEndPosition, pitch);

            SequencerNotes.AddNote(newNote);
        }

        private Position GetDefaultEndPosition(Position notePosition)
        {
            return notePosition.NextPosition(SequencerSettings.TimeSignature);
        }
    }
}