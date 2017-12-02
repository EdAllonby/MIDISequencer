using System.Windows.Input;

namespace Sequencer.View.Input
{
    public interface IKeyboardStateProcessor
    {
        bool IsKeyDown(Key key);
    }
}