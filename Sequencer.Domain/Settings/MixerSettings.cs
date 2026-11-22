using System;

namespace Sequencer.Domain.Settings
{
    /// <summary>
    /// The output: sample rate and master volume.
    /// </summary>
    public sealed class MixerSettings
    {
        private double masterVolume = 1.0;

        public int SampleRate => 44100;

        /// <summary>
        /// From 0 to 1.
        /// </summary>
        public double MasterVolume
        {
            get => masterVolume;
            set => masterVolume = Math.Clamp(value, 0, 1);
        }
    }
}
