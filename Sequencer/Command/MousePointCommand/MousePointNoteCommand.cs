using System.Windows;
using JetBrains.Annotations;
using Sequencer.Domain;
using Sequencer.Drawing;
using Sequencer.View;

namespace Sequencer.Command.MousePointCommand
{
    /// <summary>
    /// Commands for mouse driven actions to notes.
    /// </summary>
    public abstract class MousePointNoteCommand : IMousePointNoteCommand
    {
        protected readonly SequencerDimensionsCalculator SequencerDimensionsCalculator;
        protected readonly SequencerNotes SequencerNotes;
        protected readonly SequencerSettings SequencerSettings;

        protected MousePointNoteCommand([NotNull] SequencerNotes sequencerNotes, [NotNull] SequencerSettings sequencerSettings, [NotNull] SequencerDimensionsCalculator sequencerDimensionsCalculator)
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