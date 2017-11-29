using System.Windows.Input;

namespace Sequencer.Input
{
    public interface IMouseStateProcessor
    {
        bool IsButtonPressed(MouseButtonState button);
    }
}