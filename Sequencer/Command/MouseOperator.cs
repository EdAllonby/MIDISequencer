using System.Windows.Input;

namespace Sequencer.Command
{
    public static class MouseOperator
    {
        public static bool CanModifyNote => Mouse.LeftButton == MouseButtonState.Pressed;

        public static bool CanModifyContextMenu => Mouse.RightButton == MouseButtonState.Pressed;
    }
}
