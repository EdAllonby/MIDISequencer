using JetBrains.Annotations;

namespace Sequencer.Command.MousePointCommand
{
    public interface IMousePointNoteCommand
    {
        void Execute([NotNull] IMousePoint mousePoint);
    }
}