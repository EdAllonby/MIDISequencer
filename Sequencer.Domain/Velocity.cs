using Sequencer.Domain.Utility;

namespace Sequencer.Domain
{
    /// <summary>
    /// Defines a note velocity.
    /// </summary>
    public sealed class Velocity
    {
        private const int MinValue = 0;
        private const int MaxValue = 127;

        /// <summary>
        /// Create a velocity.
        /// </summary>
        /// <param name="value">The velocity value</param>
        public Velocity(int value)
        {
            Value = value.Clamp(MinValue, MaxValue);
        }

        /// <summary>
        /// The velocity value.
        /// </summary>
        public int Value { get; }

        /// <summary>
        /// The velocity as a percentage, ranging from 0 to 1.
        /// </summary>
        public double Volume => (double) Value/MaxValue;
    }
}