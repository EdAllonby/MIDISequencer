namespace Sequencer.Domain.Settings
{
    public interface IAudioSettings
    {
        int SampleRate { get; }

        Waveform Waveform { get; }

        double AttackSeconds { get; }

        double DecaySeconds { get; }

        /// <summary>
        /// Sustain level, from 0 to 1.
        /// </summary>
        double SustainLevel { get; }

        double ReleaseSeconds { get; }

        /// <summary>
        /// Output volume, from 0 to 1.
        /// </summary>
        double MasterVolume { get; }
    }
}
