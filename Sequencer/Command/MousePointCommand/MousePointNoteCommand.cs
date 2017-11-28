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
        protected readonly ISequencerDimensionsCalculator SequencerDimensionsCalculator;
        protected readonly ISequencerNotes SequencerNotes;
        protected readonly SequencerSettings SequencerSettings;

        protected MousePointNoteCommand([NotNull] ISequencerNotes sequencerNotes, [NotNull] SequencerSettings sequencerSettings, [NotNull] ISequencerDimensionsCalculator sequencerDimensionsCalculator)
        {
            SequencerNotes = sequencerNotes;
            SequencerSettings = sequencerSettings;
            SequencerDimensionsCalculator = sequencerDimensionsCalculator;
        }


        public void Execute(IMousePoint mousePoint)
        {
            if (CanExecute)
            {
                DoExecute(mousePoint);
            }
        }

        protected abstract bool CanExecute { get; }

        protected abstract void DoExecute(IMousePoint mousePoint);
    }
}