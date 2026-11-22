using System;

namespace Sequencer.Domain.Settings
{
    public enum FilterType
    {
        LowPass,
        HighPass,
        BandPass
    }

    public enum LfoTarget
    {
        Pitch,
        Filter,
        Volume
    }

    /// <summary>
    /// A subtractive synth patch: two oscillators and noise, mixed into a resonant filter with its own
    /// envelope, then an amp envelope, with an LFO for movement. Editable while the app runs: the UI thread
    /// writes these values, and each note takes a snapshot when it starts.
    /// </summary>
    public sealed class SynthSettings : IAudioSettings
    {
        public const double MinimumEnvelopeSeconds = 0.001;
        public const double MaximumEnvelopeSeconds = 3.0;
        public const double MinimumCutoff = 20;
        public const double MaximumCutoff = 18000;

        private double attackSeconds = 0.005;
        private double decaySeconds = 0.1;
        private double sustainLevel = 0.7;
        private double releaseSeconds = 0.2;
        private double volume = 0.8;

        private int oscillator2Semitones;
        private double oscillator2Detune;
        private double oscillatorMix;
        private double noiseLevel;

        private double cutoff = MaximumCutoff;
        private double resonance;
        private double filterEnvelopeAmount;
        private double keyTracking;
        private double filterAttackSeconds = 0.005;
        private double filterDecaySeconds = 0.3;
        private double filterSustainLevel = 0.5;
        private double filterReleaseSeconds = 0.3;

        private double lfoRate = 5;
        private double lfoDepth;

        public int SampleRate => 44100;

        // Oscillators.

        /// <summary>
        /// Oscillator 1's waveform.
        /// </summary>
        public Waveform Waveform { get; set; } = Waveform.Sawtooth;

        public Waveform Oscillator2Waveform { get; set; } = Waveform.Sawtooth;

        /// <summary>
        /// Oscillator 2's tuning against oscillator 1, from -24 to +24 semitones.
        /// </summary>
        public int Oscillator2Semitones
        {
            get => oscillator2Semitones;
            set => oscillator2Semitones = Math.Clamp(value, -24, 24);
        }

        /// <summary>
        /// Fine tuning of oscillator 2, from -50 to +50 cents. A few cents makes the two beat for a fuller sound.
        /// </summary>
        public double Oscillator2Detune
        {
            get => oscillator2Detune;
            set => oscillator2Detune = Math.Clamp(value, -50, 50);
        }

        /// <summary>
        /// From 0 (only oscillator 1) to 1 (only oscillator 2).
        /// </summary>
        public double OscillatorMix
        {
            get => oscillatorMix;
            set => oscillatorMix = Math.Clamp(value, 0, 1);
        }

        /// <summary>
        /// White noise added to the oscillators, from 0 to 1.
        /// </summary>
        public double NoiseLevel
        {
            get => noiseLevel;
            set => noiseLevel = Math.Clamp(value, 0, 1);
        }

        // Filter.

        public FilterType FilterType { get; set; } = FilterType.LowPass;

        /// <summary>
        /// In hertz, from 20 Hz to 18 kHz.
        /// </summary>
        public double Cutoff
        {
            get => cutoff;
            set => cutoff = Math.Clamp(value, MinimumCutoff, MaximumCutoff);
        }

        /// <summary>
        /// From 0 (none) to 1 (close to self-oscillation): emphasises frequencies around the cutoff.
        /// </summary>
        public double Resonance
        {
            get => resonance;
            set => resonance = Math.Clamp(value, 0, 1);
        }

        /// <summary>
        /// How far the filter envelope moves the cutoff, from -5 to +5 octaves.
        /// </summary>
        public double FilterEnvelopeAmount
        {
            get => filterEnvelopeAmount;
            set => filterEnvelopeAmount = Math.Clamp(value, -5, 5);
        }

        /// <summary>
        /// How much the cutoff follows the note, from 0 to 1 (1: an octave up the keyboard opens it an octave).
        /// </summary>
        public double KeyTracking
        {
            get => keyTracking;
            set => keyTracking = Math.Clamp(value, 0, 1);
        }

        public double FilterAttackSeconds
        {
            get => filterAttackSeconds;
            set => filterAttackSeconds = ClampEnvelopeTime(value);
        }

        public double FilterDecaySeconds
        {
            get => filterDecaySeconds;
            set => filterDecaySeconds = ClampEnvelopeTime(value);
        }

        public double FilterSustainLevel
        {
            get => filterSustainLevel;
            set => filterSustainLevel = Math.Clamp(value, 0, 1);
        }

        public double FilterReleaseSeconds
        {
            get => filterReleaseSeconds;
            set => filterReleaseSeconds = ClampEnvelopeTime(value);
        }

        // Amp envelope.

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

        // LFO.

        /// <summary>
        /// From 0.1 to 20 Hz.
        /// </summary>
        public double LfoRate
        {
            get => lfoRate;
            set => lfoRate = Math.Clamp(value, 0.1, 20);
        }

        /// <summary>
        /// From 0 (off) to 1. At full depth: ±1 semitone of vibrato, ±2 octaves of filter sweep, or tremolo to silence.
        /// </summary>
        public double LfoDepth
        {
            get => lfoDepth;
            set => lfoDepth = Math.Clamp(value, 0, 1);
        }

        public LfoTarget LfoTarget { get; set; } = LfoTarget.Filter;

        public double Volume
        {
            get => volume;
            set => volume = Math.Clamp(value, 0, 1);
        }

        /// <summary>
        /// Copies every setting from another patch (used to load presets).
        /// </summary>
        public void CopyFrom(SynthSettings other)
        {
            Waveform = other.Waveform;
            Oscillator2Waveform = other.Oscillator2Waveform;
            Oscillator2Semitones = other.Oscillator2Semitones;
            Oscillator2Detune = other.Oscillator2Detune;
            OscillatorMix = other.OscillatorMix;
            NoiseLevel = other.NoiseLevel;
            FilterType = other.FilterType;
            Cutoff = other.Cutoff;
            Resonance = other.Resonance;
            FilterEnvelopeAmount = other.FilterEnvelopeAmount;
            KeyTracking = other.KeyTracking;
            FilterAttackSeconds = other.FilterAttackSeconds;
            FilterDecaySeconds = other.FilterDecaySeconds;
            FilterSustainLevel = other.FilterSustainLevel;
            FilterReleaseSeconds = other.FilterReleaseSeconds;
            AttackSeconds = other.AttackSeconds;
            DecaySeconds = other.DecaySeconds;
            SustainLevel = other.SustainLevel;
            ReleaseSeconds = other.ReleaseSeconds;
            LfoRate = other.LfoRate;
            LfoDepth = other.LfoDepth;
            LfoTarget = other.LfoTarget;
            Volume = other.Volume;
        }

        private static double ClampEnvelopeTime(double seconds)
        {
            return Math.Clamp(seconds, MinimumEnvelopeSeconds, MaximumEnvelopeSeconds);
        }
    }
}
