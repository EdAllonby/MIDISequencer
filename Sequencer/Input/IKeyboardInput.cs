using System.Windows.Input;

namespace Sequencer.Input
{
    public interface IKeyboardInput
    {
        Key KeyPress { get; }
        ModifierKeys Modifiers { get; }
    }
}