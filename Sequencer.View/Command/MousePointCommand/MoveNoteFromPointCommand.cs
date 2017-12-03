using System.Windows.Input;
using JetBrains.Annotations;
using Sequencer.Domain;
using Sequencer.View.Command.NotesCommand;
using Sequencer.View.Control;
using Sequencer.View.Drawing;
using Sequencer.View.Input;

namespace Sequencer.View.Command.MousePointCommand
{
    public sealed class MoveNoteFromPointCommand : MousePointNoteCommand
    {
        [NotNull] private readonly IKeyboardStateProcessor keyboardStateProcessor;
        [NotNull] private readonly IMouseOperator mouseOperator;
        [NotNull] private readonly IPitchAndPositionCalculator pitchAndPositionCalculator;
        [NotNull] private readonly ISequencerDimensionsCalculator sequencerDimensionsCalculator;
        [NotNull] private readonly ISequencerNotes sequencerNotes;
        private int beatsDelta;
        [NotNull] private IMousePoint initialMousePitch;
        [NotNull] private IMousePoint initialMousePoint;
        private int lastHalfStepDifference;

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

            int newBeatsDelta = pitchAndPositionCalculator.FindBeatsBetweenPositions(initialPosition, newPosition);

            if (newBeatsDelta != beatsDelta)
            {
                beatsDelta = newBeatsDelta;

                initialMousePoint = mousePoint;

                var moveNotePositionCommand = new MoveNotePositionCommand(beatsDelta);
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