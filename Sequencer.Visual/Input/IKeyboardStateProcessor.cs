using System.Windows.Input;

namespace Sequencer.Visual.Input
{
    public interface IKeyboardStateProcessor
    {
        bool IsKeyDown(Key key);
    }
}