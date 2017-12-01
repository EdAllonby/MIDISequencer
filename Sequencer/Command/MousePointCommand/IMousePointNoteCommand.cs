using JetBrains.Annotations;
using Sequencer.Input;

namespace Sequencer.Command.MousePointCommand
{
    public interface IMousePointNoteCommand
    {
        void Execute([NotNull] IMousePoint mousePoint);
    }
}