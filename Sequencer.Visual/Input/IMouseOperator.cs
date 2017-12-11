namespace Sequencer.Visual.Input
{
    public interface IMouseOperator
    {
        bool CanModifyContextMenu { get; }
        bool CanModifyNote { get; }
    }
}