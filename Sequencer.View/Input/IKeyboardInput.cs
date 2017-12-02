using System.Windows.Input;

namespace Sequencer.View.Input
{
    public interface IKeyboardInput
    {
        Key KeyPress { get; }
        ModifierKeys Modifiers { get; }
    }
}