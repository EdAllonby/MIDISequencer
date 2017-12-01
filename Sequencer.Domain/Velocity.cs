using System;
using JetBrains.Annotations;
using Sequencer.Utilities;

namespace Sequencer.Domain
{
    /// <summary>
    /// Defines a note velocity.
    /// </summary>
    public sealed class Velocity : IEquatable<Velocity>
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
        private int Value { get; }

        /// <summary>
        /// The velocity as a percentage, ranging from 0 to 1.
        /// </summary>
        public double Volume => (double) Value / MaxValue;

        public bool Equals(Velocity other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Value == other.Value;
        }

        [NotNull]
        public static Velocity operator +([NotNull] Velocity velocity, int addition)
        {
            return new Velocity(velocity.Value + addition);
        }

        [NotNull]
        public static Velocity operator -([NotNull] Velocity velocity, int addition)
        {
            return new Velocity(velocity.Value - addition);
        }

        public override string ToString()
        {
            return Value.ToString();
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj is Velocity && Equals((Velocity) obj);
        }

        public override int GetHashCode()
        {
            return Value;
        }
    }
}