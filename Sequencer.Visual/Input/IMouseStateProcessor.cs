using System.Windows.Input;

namespace Sequencer.Visual.Input
{
    public interface IMouseStateProcessor
    {
        bool IsButtonPressed(MouseButtonState button);
    }
}