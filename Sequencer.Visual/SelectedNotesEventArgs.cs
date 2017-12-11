using System;
using System.Collections.Generic;
using JetBrains.Annotations;

namespace Sequencer.Visual
{
    public sealed class SelectedNotesEventArgs : EventArgs
    {
        [NotNull]
        [ItemNotNull]
        public IEnumerable<IVisualNote> SelectedVisualNotes { get; }

        public SelectedNotesEventArgs([NotNull] [ItemNotNull] IEnumerable<IVisualNote> selectedVisualNotes)
        {
            SelectedVisualNotes = selectedVisualNotes;
        }
    }
}