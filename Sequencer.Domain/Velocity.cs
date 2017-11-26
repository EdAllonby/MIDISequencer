﻿using System;
using Sequencer.Utilities;

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

        public static Velocity operator +(Velocity velocity, int addition)
        {
            return new Velocity(velocity.Value + addition);
        }

        public static Velocity operator -(Velocity velocity, int addition)
        {
            return new Velocity(velocity.Value - addition);
        }

        /// <summary>
        /// The velocity as a percentage, ranging from 0 to 1.
        /// </summary>
        public double Volume => (double) Value / MaxValue;

        public override string ToString()
        {
            return Value.ToString();
        }
    }
}