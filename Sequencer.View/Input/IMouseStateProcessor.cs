using System.Windows.Input;

namespace Sequencer.View.Input
{
    public interface IMouseStateProcessor
    {
        bool IsButtonPressed(MouseButtonState button);
    }
}