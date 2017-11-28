using System.Windows;

namespace Sequencer.Command.MousePointCommand
{
    public interface IMousePointNoteCommand
    {
        void Execute(IMousePoint mousePoint);
    }
}