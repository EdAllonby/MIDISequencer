using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using JetBrains.Annotations;
using Sequencer.Domain;

namespace Sequencer.Command
{
    public sealed class MousePointMoveNoteCommand : MousePointNoteCommand
    {
        private int beatsDelta;
        private Point initialMousePitch;
        private Point initialMousePosition;
        private int midiPitchDelta;

        public MousePointMoveNoteCommand(Point initialMousePoint, [NotNull] List<VisualNote> sequencerNotes, [NotNull] SequencerSettings sequencerSettings,
            [NotNull] SequencerDimensionsCalculator sequencerDimensionsCalculator) : base(sequencerNotes, sequencerSettings, sequencerDimensionsCalculator)
        {
            initialMousePosition = initialMousePoint;
            initialMousePitch = initialMousePoint;
        }

        public override void Execute(Point mousePoint)
        {
            MoveNotePositions(mousePoint);

            if (!Keyboard.IsKeyDown(Key.LeftShift))
            {
                MoveNotePitch(mousePoint);
            }
        }

        private void MoveNotePositions(Point mousePoint)
        {
            Position initialPosition = sequencerDimensionsCalculator.FindPositionFromPoint(initialMousePosition);
            Position newPosition = sequencerDimensionsCalculator.FindPositionFromPoint(mousePoint);

            int newBeatsDelta = newPosition.SummedBeat(sequencerSettings.TimeSignature) -
                                initialPosition.SummedBeat(sequencerSettings.TimeSignature);

            if (newBeatsDelta != beatsDelta)
            {
                beatsDelta = newBeatsDelta;

                initialMousePosition = mousePoint;

                foreach (VisualNote visualNote in sequencerNotes.Where(note => note.NoteState == NoteState.Selected))
                {
                    visualNote.MovePositionRelativeTo(beatsDelta);
                }
            }
        }

        private void MoveNotePitch(Point mousePoint)
        {
            Pitch initialPitch = sequencerDimensionsCalculator.FindPitchFromPoint(initialMousePitch);
            Pitch newPitch = sequencerDimensionsCalculator.FindPitchFromPoint(mousePoint);

            int newMidiPitchDelta = newPitch.MidiNoteNumber - initialPitch.MidiNoteNumber;

            if (newMidiPitchDelta != midiPitchDelta)
            {
                midiPitchDelta = newMidiPitchDelta;

                initialMousePitch = mousePoint;

                foreach (VisualNote visualNote in sequencerNotes.Where(note => note.NoteState == NoteState.Selected))
                {
                    visualNote.MovePitchRelativeTo(midiPitchDelta);
                }
            }
        }
    }
}