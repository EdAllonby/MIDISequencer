using System.Windows.Input;

namespace Sequencer.Input
{
    public class MouseOperator : IMouseOperator
    {
        private readonly IMouseStateProcessor mouseStateProcessor;

        public MouseOperator(IMouseStateProcessor mouseStateProcessor)
        {
            this.mouseStateProcessor = mouseStateProcessor;
        }

        public bool CanModifyNote => mouseStateProcessor.IsButtonPressed(Mouse.LeftButton);

        public bool CanModifyContextMenu => mouseStateProcessor.IsButtonPressed(Mouse.RightButton);
    }
}
