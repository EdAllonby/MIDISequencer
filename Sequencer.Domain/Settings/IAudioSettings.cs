namespace Sequencer.Domain.Settings
{
    public interface IAudioSettings
    {
        int SampleRate { get; }

        double AttackSeconds { get; }

        double DecaySeconds { get; }

        /// <summary>
        /// Sustain level, from 0 to 1.
        /// </summary>
        double SustainLevel { get; }

        double ReleaseSeconds { get; }
    }
}
