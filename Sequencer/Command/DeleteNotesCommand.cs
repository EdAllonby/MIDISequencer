using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;
using JetBrains.Annotations;
using log4net;

namespace Sequencer.Command
{
    public sealed class DeleteNotesCommand
    {
        private readonly ILog Log = LogManager.GetLogger(typeof(DeleteNotesCommand));
        private readonly Canvas sequencerCanvas;
        private readonly List<VisualNote> sequencerNotes;

        public DeleteNotesCommand([NotNull] Canvas sequencerCanvas, [NotNull] List<VisualNote> sequencerNotes)
        {
            this.sequencerCanvas = sequencerCanvas;
            this.sequencerNotes = sequencerNotes;
        }

        public void Execute(IEnumerable<VisualNote> notesToDelete)
        {
            foreach (VisualNote noteToDelete in notesToDelete)
            {
                if (noteToDelete != null)
                {
                    noteToDelete.Remove(sequencerCanvas);
                    Log.Info($"Visual note [{noteToDelete}] has been deleted.");
                }
            }

            sequencerNotes.RemoveAll(notesToDelete.Contains);
        }
    }
}