using System.Windows.Input;

namespace Sequencer.Input
{
    public interface IKeyboardStateProcessor
    {
        bool IsKeyDown(Key key);
    }
}