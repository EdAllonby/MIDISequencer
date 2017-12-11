using JetBrains.Annotations;
using Sequencer.Visual.Input;

namespace Sequencer.View.Command.MousePointCommand
{
    public interface IMousePointNoteCommand
    {
        void Execute([NotNull] IMousePoint mousePoint);
    }
}