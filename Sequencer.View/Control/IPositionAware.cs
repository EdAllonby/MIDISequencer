using System.Windows.Media;
using JetBrains.Annotations;

namespace Sequencer.View.Control
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
        bool IntersectsWith([NotNull] Geometry geometry);
    }
}