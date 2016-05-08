using System.Collections.Generic;
using System.Windows;
using JetBrains.Annotations;
using log4net;

namespace Sequencer.Command
{
    /// <summary>
    /// Commands for mouse driven actions to notes.
    /// </summary>
    public abstract class MousePointNoteCommand
    {
        protected static readonly ILog Log = LogManager.GetLogger(typeof(MousePointNoteCommand));

        protected readonly SequencerDimensionsCalculator sequencerDimensionsCalculator;
        protected readonly List<VisualNote> sequencerNotes;
        protected readonly SequencerSettings sequencerSettings;

        protected MousePointNoteCommand([NotNull] List<VisualNote> sequencerNotes, [NotNull] SequencerSettings sequencerSettings, [NotNull] SequencerDimensionsCalculator sequencerDimensionsCalculator)
        {
            this.sequencerNotes = sequencerNotes;
            this.sequencerSettings = sequencerSettings;
            this.sequencerDimensionsCalculator = sequencerDimensionsCalculator;
        }


        public void Execute(Point mousePoint)
        {
            if (CanExecute())
            {
                DoExecute(mousePoint);
            }
        }

        protected abstract bool CanExecute();
        
        protected abstract void DoExecute(Point mousePoint);
    }
}