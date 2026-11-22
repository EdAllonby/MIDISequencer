using Avalonia;

namespace Sequencer.Visual
{
    public interface IPositionAware
    {
        bool IntersectsWith(Rect rectangle);
    }
}
