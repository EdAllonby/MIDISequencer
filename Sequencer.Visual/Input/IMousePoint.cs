using System.Windows;

namespace Sequencer.Visual.Input
{
    public interface IMousePoint
    {
        double X { get; }
        double Y { get; }

        Point Point { get; }
    }
}