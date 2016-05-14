using System.Collections.Generic;
using System.Windows;
using JetBrains.Annotations;

namespace Sequencer.Command.MousePointCommand
{
    /// <summary>
    /// Commands for mouse driven actions to notes.
    /// </summary>
    public abstract class MousePointNoteCommand : IMousePointNoteCommand
    {
        protected readonly SequencerDimensionsCalculator SequencerDimensionsCalculator;
        protected readonly List<VisualNote> SequencerNotes;
        protected readonly SequencerSettings SequencerSettings;

        protected MousePointNoteCommand([NotNull] List<VisualNote> sequencerNotes, [NotNull] SequencerSettings sequencerSettings, [NotNull] SequencerDimensionsCalculator sequencerDimensionsCalculator)
        {
            SequencerNotes = sequencerNotes;
            SequencerSettings = sequencerSettings;
            SequencerDimensionsCalculator = sequencerDimensionsCalculator;
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