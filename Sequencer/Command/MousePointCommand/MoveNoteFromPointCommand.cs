using System.Windows.Input;
using JetBrains.Annotations;
using Sequencer.Command.NotesCommand;
using Sequencer.Domain;
using Sequencer.Drawing;
using Sequencer.Input;
using Sequencer.View;

namespace Sequencer.Command.MousePointCommand
{
    public sealed class MoveNoteFromPointCommand : MousePointNoteCommand
    {
        [NotNull] private readonly IMouseOperator mouseOperator;
        [NotNull] private readonly ISequencerDimensionsCalculator sequencerDimensionsCalculator;
        [NotNull] private readonly ISequencerNotes sequencerNotes;
        [NotNull] private readonly SequencerSettings sequencerSettings;
        private int beatsDelta;
        private IMousePoint initialMousePitch;
        private IMousePoint initialMousePoint;
        private int lastHalfStepDifference;

        public MoveNoteFromPointCommand([NotNull] IMousePoint initialMousePoint, [NotNull] IMouseOperator mouseOperator,
            [NotNull] ISequencerNotes sequencerNotes, [NotNull] SequencerSettings sequencerSettings,
            [NotNull] ISequencerDimensionsCalculator sequencerDimensionsCalculator)
        {
            this.initialMousePoint = initialMousePoint;
            this.mouseOperator = mouseOperator;
            this.sequencerNotes = sequencerNotes;
            this.sequencerSettings = sequencerSettings;
            this.sequencerDimensionsCalculator = sequencerDimensionsCalculator;
            initialMousePitch = initialMousePoint;
        }

        protected override bool CanExecute => mouseOperator.CanModifyNote;

        private void MoveNotePositions(IMousePoint mousePoint)
        {
            Position initialPosition = sequencerDimensionsCalculator.FindPositionFromPoint(initialMousePoint);
            Position newPosition = sequencerDimensionsCalculator.FindPositionFromPoint(mousePoint);

            int newBeatsDelta = newPosition.SummedBeat(sequencerSettings.TimeSignature) -
                                initialPosition.SummedBeat(sequencerSettings.TimeSignature);

            if (newBeatsDelta != beatsDelta)
            {
                beatsDelta = newBeatsDelta;

                initialMousePoint = mousePoint;

                var moveNotePositionCommand = new MoveNotePositionCommand(beatsDelta);
                moveNotePositionCommand.Execute(sequencerNotes.SelectedNotes);
            }
        }

        private void MoveNotePitch(IMousePoint mousePoint)
        {
            Pitch initialPitch = sequencerDimensionsCalculator.FindPitchFromPoint(initialMousePitch);
            Pitch newPitch = sequencerDimensionsCalculator.FindPitchFromPoint(mousePoint);

            int halfStepDifference = PitchStepCalculator.FindStepsFromPitches(initialPitch, newPitch);

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

            if (!Keyboard.IsKeyDown(Key.LeftShift))
            {
                MoveNotePitch(mousePoint);
            }
        }
    }
}