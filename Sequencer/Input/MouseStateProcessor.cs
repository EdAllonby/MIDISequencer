using System.Windows.Input;

namespace Sequencer.Input
{
    public class MouseStateProcessor : IMouseStateProcessor
    {
        public bool IsButtonPressed(MouseButtonState button)
        {
            return button == MouseButtonState.Pressed;
        }
    }
}