using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using JetBrains.Annotations;
using log4net;
using Sequencer.Domain;

namespace Sequencer.Command
{
    public sealed class UpdateNoteStateCommand : NoteCommand
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(UpdateNoteStateCommand));

        public UpdateNoteStateCommand([NotNull] List<VisualNote> sequencerNotes, [NotNull] SequencerSettings sequencerSettings, [NotNull] SequencerDimensionsCalculator sequencerDimensionsCalculator)
            : base(sequencerNotes, sequencerSettings, sequencerDimensionsCalculator)
        {
        }

        public override void Execute(Point mousePoint)
        {
            VisualNote actionableNote = FindNoteFromPoint(mousePoint);

            if (actionableNote != null)
            {
                if (!Keyboard.IsKeyDown(Key.LeftCtrl))
                {
                    foreach (VisualNote visualNote in sequencerNotes.Where(x => x != actionableNote))
                    {
                        visualNote.NoteState = NoteState.Unselected;
                    }
                }

                actionableNote.NoteState = actionableNote.NoteState == NoteState.Selected ? NoteState.Unselected : NoteState.Selected;

                Log.Info($"Note {actionableNote} has been {actionableNote.NoteState}");
            }
        }

        private VisualNote FindNoteFromPoint(Point point)
        {
            Position mousePosition = sequencerDimensionsCalculator.FindNotePositionFromPoint(point);
            Pitch mousePitch = sequencerDimensionsCalculator.FindPitch(point);
            return sequencerNotes.FirstOrDefault(DoesPitchAndPositionMatchCurrentNote(mousePosition, mousePitch));
        }


        private static Func<VisualNote, bool> DoesPitchAndPositionMatchCurrentNote(Position mousePosition, Pitch mousePitch)
        {
            return visualNote => (visualNote.StartPosition <= mousePosition) && (visualNote.EndPosition > mousePosition) && visualNote.Pitch.Equals(mousePitch);
        }
    }
}