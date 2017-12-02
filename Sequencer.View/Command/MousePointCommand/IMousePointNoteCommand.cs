using JetBrains.Annotations;
using Sequencer.View.Input;

namespace Sequencer.View.Command.MousePointCommand
{
    public interface IMousePointNoteCommand
    {
        void Execute([NotNull] IMousePoint mousePoint);
    }
}