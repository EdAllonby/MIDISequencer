using System;

namespace Sequencer.Domain.Settings
{
    /// <summary>
    /// The synthesiser's sound, editable while the app runs. The UI thread writes these values; the audio
    /// side reads the waveform and envelope when a note starts, and the volume on every buffer.
    /// </summary>
    public sealed class SynthSettings : IAudioSettings
    {
        public const double MinimumEnvelopeSeconds = 0.001;
        public const double MaximumEnvelopeSeconds = 3.0;

        private double attackSeconds = 0.005;
        private double decaySeconds = 0.1;
        private double sustainLevel = 0.7;
        private double releaseSeconds = 0.2;
        private double masterVolume = 1.0;

        public int SampleRate => 44100;

        public Waveform Waveform { get; set; } = Waveform.Sawtooth;

        public double AttackSeconds
        {
            get => attackSeconds;
            set => attackSeconds = ClampEnvelopeTime(value);
        }

        public double DecaySeconds
        {
            get => decaySeconds;
            set => decaySeconds = ClampEnvelopeTime(value);
        }

        public double SustainLevel
        {
            get => sustainLevel;
            set => sustainLevel = Math.Clamp(value, 0, 1);
        }

        public double ReleaseSeconds
        {
            get => releaseSeconds;
            set => releaseSeconds = ClampEnvelopeTime(value);
        }

        public double MasterVolume
        {
            get => masterVolume;
            set => masterVolume = Math.Clamp(value, 0, 1);
        }

        private static double ClampEnvelopeTime(double seconds)
        {
            return Math.Clamp(seconds, MinimumEnvelopeSeconds, MaximumEnvelopeSeconds);
        }
    }
}
