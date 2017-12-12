using JetBrains.Annotations;
using Sequencer.Domain;
using Sequencer.Domain.Settings;
using Sequencer.Visual;
using Sequencer.Visual.Input;

namespace Sequencer.View.Command.MousePointCommand
{
    /// <summary>
    /// Creates a <see cref="VisualNote" /> with correct pitch relative to the sequencer.
    /// </summary>
    public class CreateNoteFromPointCommand : MousePointNoteCommand
    {
        [NotNull] private readonly IMouseOperator mouseOperator;
        [NotNull] private readonly ISequencerDimensionsCalculator sequencerDimensionsCalculator;
        [NotNull] private readonly ISequencerNotes sequencerNotes;
        [NotNull] private readonly SequencerSettings sequencerSettings;
        [NotNull] private readonly IVisualNoteFactory visualNoteFactory;

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

        [NotNull]
        private IPosition GetDefaultEndPosition([NotNull] IPosition notePosition)
        {
            return notePosition.NextPosition(sequencerSettings.TimeSignature);
        }

        protected override void DoExecute(IMousePoint mousePoint)
        {
            sequencerNotes.MakeAllUnselected();
            IPosition notePosition = sequencerDimensionsCalculator.FindPositionFromPoint(mousePoint);
            Pitch pitch = sequencerDimensionsCalculator.FindPitchFromPoint(mousePoint);

            IPosition defaultEndPosition = GetDefaultEndPosition(notePosition);

            IVisualNote newNote = visualNoteFactory.CreateNote(pitch, notePosition, defaultEndPosition);

            sequencerNotes.AddNote(newNote);
        }
    }
}