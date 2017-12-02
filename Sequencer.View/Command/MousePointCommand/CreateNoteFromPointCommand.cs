using JetBrains.Annotations;
using Sequencer.Domain;
using Sequencer.Shared;
using Sequencer.View.Drawing;
using Sequencer.View.Input;
using Sequencer.View.Control;

namespace Sequencer.View.Command.MousePointCommand
{
    /// <summary>
    /// Creates a <see cref="VisualNote" /> with correct pitch relative to the sequencer.
    /// </summary>
    public class CreateNoteFromPointCommand : MousePointNoteCommand
    {
        [NotNull] private readonly IVisualNoteFactory visualNoteFactory;
        [NotNull] private readonly ISequencerNotes sequencerNotes;
        [NotNull] private readonly SequencerSettings sequencerSettings;
        [NotNull] private readonly IMouseOperator mouseOperator;
        [NotNull] private readonly ISequencerDimensionsCalculator sequencerDimensionsCalculator;

        public CreateNoteFromPointCommand([NotNull] IVisualNoteFactory visualNoteFactory, [NotNull] ISequencerNotes sequencerNotes,
            [NotNull] SequencerSettings sequencerSettings, [NotNull] IMouseOperator mouseOperator,
            [NotNull] ISequencerDimensionsCalculator sequencerDimensionsCalculator)
        {
            this.visualNoteFactory = visualNoteFactory;
            this.sequencerNotes = sequencerNotes;
            this.sequencerSettings = sequencerSettings;
            this.mouseOperator = mouseOperator;
            this.sequencerDimensionsCalculator = sequencerDimensionsCalculator;
        }

        protected override bool CanExecute => mouseOperator.CanModifyNote;

        protected override void DoExecute(IMousePoint mousePoint)
        {
            sequencerNotes.MakeAllUnselected();
            IPosition notePosition = sequencerDimensionsCalculator.FindPositionFromPoint(mousePoint);
            Pitch pitch = sequencerDimensionsCalculator.FindPitchFromPoint(mousePoint);

            IPosition defaultEndPosition = GetDefaultEndPosition(notePosition);

            IVisualNote newNote = visualNoteFactory.CreateNote(pitch, notePosition, defaultEndPosition);

            sequencerNotes.AddNote(newNote);
        }

        [NotNull]
        private IPosition GetDefaultEndPosition([NotNull] IPosition notePosition)
        {
            return notePosition.NextPosition(sequencerSettings.TimeSignature);
        }
    }
} 