using System.Windows.Input;
using JetBrains.Annotations;

namespace Sequencer.View.Input
{
    public class MouseOperator : IMouseOperator
    {
        [NotNull] private readonly IMouseStateProcessor mouseStateProcessor;

        public MouseOperator([NotNull] IMouseStateProcessor mouseStateProcessor)
        {
            this.mouseStateProcessor = mouseStateProcessor;
        }

        public bool CanModifyNote => mouseStateProcessor.IsButtonPressed(Mouse.LeftButton);

        public bool CanModifyContextMenu => mouseStateProcessor.IsButtonPressed(Mouse.RightButton);
    }
}
