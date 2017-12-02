using System.Windows;

namespace Sequencer.View.Input
{
    public interface IMousePoint
    {
        double X { get; }
        double Y { get; }

        Point Point { get; }
    }
}