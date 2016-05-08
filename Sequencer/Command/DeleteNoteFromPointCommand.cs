using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using JetBrains.Annotations;
using Sequencer.Domain;

namespace Sequencer.Command
{
    public sealed class DeleteNoteFromPointCommand : MousePointNoteCommand
    {
        private readonly Canvas sequencerCanvas;

        public DeleteNoteFromPointCommand([NotNull] Canvas sequencerCanvas, [NotNull] List<VisualNote> sequencerNotes, [NotNull] SequencerSettings sequencerSettings, [NotNull] SequencerDimensionsCalculator sequencerDimensionsCalculator) : base(sequencerNotes, sequencerSettings, sequencerDimensionsCalculator)
        {
            this.sequencerCanvas = sequencerCanvas;
        }

        public override void Execute(Point mousePoint)
        {
            VisualNote noteToDelete = sequencerDimensionsCalculator.FindNoteFromPoint(sequencerNotes, mousePoint);
            if (noteToDelete != null)
            {
                noteToDelete.Remove(sequencerCanvas);
                sequencerNotes.Remove(noteToDelete);
                Log.Info($"Removed visual note from sequencer [{noteToDelete}]");
            }
        }
    }
}