using System.Windows.Media;

namespace Sequencer.View
{
    /// <summary>
    /// An element which is aware of its position.
    /// </summary>
    public interface IPositionAware
    {
        /// <summary>
        /// Is the element intersecting with a rectangle?
        /// </summary>
        /// <param name="geometry">The rectangle to check intersection.</param>
        /// <returns>If the element intersects with the rectangle.</returns>
        bool IntersectsWith(Geometry geometry);
    }
}