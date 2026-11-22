using Avalonia.Input;

namespace Sequencer.Visual.Input
{
    public interface IKeyboardInput
    {
        Key KeyPress { get; }

        KeyModifiers Modifiers { get; }
    }
}
