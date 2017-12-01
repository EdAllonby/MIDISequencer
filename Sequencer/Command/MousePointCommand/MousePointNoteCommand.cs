using JetBrains.Annotations;
using Sequencer.Input;

namespace Sequencer.Command.MousePointCommand
{
    /// <summary>
    /// Commands for mouse driven actions to notes.
    /// </summary>
    public abstract class MousePointNoteCommand : IMousePointNoteCommand
    {
        public void Execute(IMousePoint mousePoint)
        {
            if (CanExecute)
            {
                DoExecute(mousePoint);
            }
        }

        protected abstract bool CanExecute { get; }

        protected abstract void DoExecute([NotNull] IMousePoint mousePoint);
    }
}