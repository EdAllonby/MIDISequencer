using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using JetBrains.Annotations;
using Sequencer.Domain;

namespace Sequencer.Command
{
    public sealed class DeleteNoteCommand : NoteCommand
    {
        private readonly Canvas sequencerCanvas;

        public DeleteNoteCommand([NotNull] Canvas sequencerCanvas, [NotNull] List<VisualNote> sequencerNotes, [NotNull] SequencerSettings sequencerSettings, [NotNull] SequencerDimensionsCalculator sequencerDimensionsCalculator) : base(sequencerNotes, sequencerSettings, sequencerDimensionsCalculator)
        {
            this.sequencerCanvas = sequencerCanvas;
        }

        public override void Execute(Point mousePoint)
        {
            VisualNote noteToDelete = FindNoteFromPoint(mousePoint);
            if (noteToDelete != null)
            {
                noteToDelete.Remove(sequencerCanvas);
                sequencerNotes.Remove(noteToDelete);
                Log.Info($"Removed visual note from sequencer [{noteToDelete}]");
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
            return visualNote => visualNote.StartPosition <= mousePosition && visualNote.EndPosition > mousePosition && visualNote.Pitch.Equals(mousePitch);
        }
    }
}
