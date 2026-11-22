using System;

namespace Sequencer.Domain.Drums
{
    /// <summary>
    /// The drum kit's mix settings. Read by playback each time a hit starts.
    /// </summary>
    public sealed class DrumKitSettings
    {
        private double volume = 0.8;

        /// <summary>
        /// From 0 to 1.
        /// </summary>
        public double Volume
        {
            get => volume;
            set => volume = Math.Clamp(value, 0, 1);
        }
    }
}
