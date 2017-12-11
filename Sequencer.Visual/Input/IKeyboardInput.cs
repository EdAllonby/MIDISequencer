using System.Windows.Input;

namespace Sequencer.Visual.Input
{
    public interface IKeyboardInput
    {
        Key KeyPress { get; }
        ModifierKeys Modifiers { get; }
    }
}