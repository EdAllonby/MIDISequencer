using System.Windows;
using System.Windows.Input;
using JetBrains.Annotations;
using Sequencer.Command.NotesCommand;
using Sequencer.Domain;
using Sequencer.Drawing;
using Sequencer.View;

namespace Sequencer.Command.MousePointCommand
{
    public sealed class MoveNoteFromPointCommand : MousePointNoteCommand
    {
        private int beatsDelta;
        private Point initialMousePitch;
        private Point initialMousePosition;
        private int lastHalfStepDifference;

        public MoveNoteFromPointCommand(Point initialMousePoint, [NotNull] SequencerNotes sequencerNotes, [NotNull] SequencerSettings sequencerSettings,
            [NotNull] SequencerDimensionsCalculator sequencerDimensionsCalculator) : base(sequencerNotes, sequencerSettings, sequencerDimensionsCalculator)
        {
            initialMousePosition = initialMousePoint;
            initialMousePitch = initialMousePoint;
        }

        protected override bool CanExecute()
        {
            return Mouse.LeftButton == MouseButtonState.Pressed;
        }

        protected override void DoExecute(Point mousePoint)
        {
            MoveNotePositions(mousePoint);

            if (!Keyboard.IsKeyDown(Key.LeftShift))
            {
                MoveNotePitch(mousePoint);
            }
        }

        private void MoveNotePositions(Point mousePoint)
        {
            Position initialPosition = SequencerDimensionsCalculator.FindPositionFromPoint(initialMousePosition);
            Position newPosition = SequencerDimensionsCalculator.FindPositionFromPoint(mousePoint);

            int newBeatsDelta = newPosition.SummedBeat(SequencerSettings.TimeSignature) -
                                initialPosition.SummedBeat(SequencerSettings.TimeSignature);

            if (newBeatsDelta != beatsDelta)
            {
                beatsDelta = newBeatsDelta;

                initialMousePosition = mousePoint;

                var moveNotePositionCommand = new MoveNotePositionCommand(beatsDelta);
                moveNotePositionCommand.Execute(SequencerNotes.SelectedNotes);
            }
        }

        private void MoveNotePitch(Point mousePoint)
        {
            Pitch initialPitch = SequencerDimensionsCalculator.FindPitchFromPoint(initialMousePitch);
            Pitch newPitch = SequencerDimensionsCalculator.FindPitchFromPoint(mousePoint);

            int halfStepDifference = PitchStepCalculator.FindStepsFromPitches(initialPitch, newPitch);

            if (halfStepDifference != lastHalfStepDifference)
            {
                lastHalfStepDifference = halfStepDifference;

                initialMousePitch = mousePoint;

                var moveNotePitchCommand = new MoveNotePitchCommand(lastHalfStepDifference);
                moveNotePitchCommand.Execute(SequencerNotes.SelectedNotes);
            }
        }
    }
}