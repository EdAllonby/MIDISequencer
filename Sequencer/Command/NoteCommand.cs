using System.Collections.Generic;
using System.Windows;
using JetBrains.Annotations;

namespace Sequencer.Command
{
    public abstract class NoteCommand
    {
        protected readonly SequencerDimensionsCalculator sequencerDimensionsCalculator;
        protected readonly List<VisualNote> sequencerNotes;
        protected readonly SequencerSettings sequencerSettings;

        protected NoteCommand([NotNull]List<VisualNote> sequencerNotes, [NotNull] SequencerSettings sequencerSettings, [NotNull] SequencerDimensionsCalculator sequencerDimensionsCalculator)
        {
            this.sequencerNotes = sequencerNotes;
            this.sequencerSettings = sequencerSettings;
            this.sequencerDimensionsCalculator = sequencerDimensionsCalculator;
        }

        public abstract void Execute(Point mousePoint);
    }
}