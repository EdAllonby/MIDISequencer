namespace Sequencer.View.Input
{
    public interface IMouseOperator
    {
        bool CanModifyContextMenu { get; }
        bool CanModifyNote { get; }
    }
}