namespace Sequencer.Audio
{
    /// <summary>
    /// How loud each source is before the master bus. Voices add up, so each needs headroom: at these levels a
    /// typical song (a lead, a pad and drums) peaks below full scale, and the limiter is a safety net rather
    /// than something working all the time (which distorts kicks and bass).
    /// </summary>
    public static class MixLevels
    {
        /// <summary>
        /// A synth note at full velocity and full track volume, about -10.5 dB.
        /// </summary>
        public const float SynthVoice = 0.3f;

        /// <summary>
        /// A drum hit at full velocity and full kit volume, about -7 dB.
        /// </summary>
        public const float DrumHit = 0.45f;
    }
}
