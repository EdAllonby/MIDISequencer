using System.Windows.Input;

namespace Sequencer.Input
{
    public static class MouseOperator
    {
        public static bool CanModifyNote => Mouse.LeftButton == MouseButtonState.Pressed;

        public static bool CanModifyContextMenu => Mouse.RightButton == MouseButtonState.Pressed;
    }
}
