using System.Windows.Input;
using JetBrains.Annotations;
using Sequencer.Domain;
using Sequencer.View.Command.NotesCommand;
using Sequencer.Visual;
using Sequencer.Visual.Input;

namespace Sequencer.View.Command.MousePointCommand
{
    public sealed class MoveNoteFromPointCommand : MousePointNoteCommand
    {
        [NotNull] private readonly IKeyboardStateProcessor keyboardStateProcessor;
        [NotNull] private readonly IMouseOperator mouseOperator;
        [NotNull] private readonly IPitchAndPositionCalculator pitchAndPositionCalculator;
        [NotNull] private readonly ISequencerDimensionsCalculator sequencerDimensionsCalculator;
        [NotNull] private readonly ISequencerNotes sequencerNotes;
        [NotNull] private IMousePoint initialMousePitch;
        [NotNull] private IMousePoint initialMousePoint;
        private int lastHalfStepDifference;
        private int ticksDelta;

        public MoveNoteFromPointCommand([NotNull] IKeyboardStateProcessor keyboardStateProcessor,
            [NotNull] IPitchAndPositionCalculator pitchAndPositionCalculator, [NotNull] IMousePoint initialMousePoint,
            [NotNull] IMouseOperator mouseOperator, [NotNull] ISequencerNotes sequencerNotes,
            [NotNull] ISequencerDimensionsCalculator sequencerDimensionsCalculator)
        {
            this.keyboardStateProcessor = keyboardStateProcessor;
            this.pitchAndPositionCalculator = pitchAndPositionCalculator;
            this.initialMousePoint = initialMousePoint;
            this.mouseOperator = mouseOperator;
            this.sequencerNotes = sequencerNotes;
            this.sequencerDimensionsCalculator = sequencerDimensionsCalculator;
            initialMousePitch = initialMousePoint;
        }

        protected override bool CanExecute => mouseOperator.CanModifyNote;

        private void MoveNotePositions([NotNull] IMousePoint mousePoint)
        {
            IPosition initialPosition = sequencerDimensionsCalculator.FindPositionFromPoint(initialMousePoint);
            IPosition newPosition = sequencerDimensionsCalculator.FindPositionFromPoint(mousePoint);

            int newTicksDelta = pitchAndPositionCalculator.FindTicksBetweenPositions(initialPosition, newPosition);

            if (newTicksDelta != ticksDelta)
            {
                ticksDelta = newTicksDelta;

                initialMousePoint = mousePoint;

                var moveNotePositionCommand = new MoveNotePositionCommand(ticksDelta);
                moveNotePositionCommand.Execute(sequencerNotes.SelectedNotes);
            }
        }

        private void MoveNotePitch([NotNull] IMousePoint mousePoint)
        {
            Pitch initialPitch = sequencerDimensionsCalculator.FindPitchFromPoint(initialMousePitch);
            Pitch newPitch = sequencerDimensionsCalculator.FindPitchFromPoint(mousePoint);

            int halfStepDifference = pitchAndPositionCalculator.FindStepsFromPitches(initialPitch, newPitch);

            if (halfStepDifference != lastHalfStepDifference)
            {
                lastHalfStepDifference = halfStepDifference;

                initialMousePitch = mousePoint;

                var moveNotePitchCommand = new MoveNotePitchCommand(lastHalfStepDifference);
                moveNotePitchCommand.Execute(sequencerNotes.SelectedNotes);
            }
        }

        protected override void DoExecute(IMousePoint mousePoint)
        {
            MoveNotePositions(mousePoint);

            if (!keyboardStateProcessor.IsKeyDown(Key.LeftShift))
            {
                MoveNotePitch(mousePoint);
            }
        }
    }
}