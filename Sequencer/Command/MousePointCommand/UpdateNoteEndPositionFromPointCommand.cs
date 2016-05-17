using System.Windows;
using System.Windows.Input;
using JetBrains.Annotations;
using Sequencer.Domain;
using Sequencer.Drawing;
using Sequencer.View;

namespace Sequencer.Command.MousePointCommand
{
    /// <summary>
    /// Command to update a <see cref="VisualNote" />'s end position based on the current mouse position in the sequencer.
    /// </summary>
    public sealed class UpdateNoteEndPositionFromPointCommand : MousePointNoteCommand
    {
        public UpdateNoteEndPositionFromPointCommand([NotNull] SequencerNotes sequencerNotes,
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
            Position currentEndPosition = SequencerDimensionsCalculator.FindPositionFromPoint(mousePosition);
            Position nextPosition = currentEndPosition.NextPosition(SequencerSettings.TimeSignature);

            foreach (VisualNote selectedNote in SequencerNotes.SelectedNotes)
            {
                selectedNote.EndPosition = nextPosition;
            }
        }
    }
}