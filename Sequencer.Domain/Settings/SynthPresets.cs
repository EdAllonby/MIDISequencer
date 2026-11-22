using System;
using System.Collections.Generic;
using JetBrains.Annotations;

namespace Sequencer.Domain.Settings
{
    /// <summary>
    /// A named starting point for the synth.
    /// </summary>
    public sealed class SynthPreset
    {
        [NotNull] private readonly Action<SynthSettings> configure;

        public SynthPreset([NotNull] string name, [NotNull] string description, [NotNull] Action<SynthSettings> configure)
        {
            Name = name;
            Description = description;
            this.configure = configure;
        }

        [NotNull]
        public string Name { get; }

        [NotNull]
        public string Description { get; }

        /// <summary>
        /// The preset's full patch: every setting starts from the defaults, then the preset changes some.
        /// </summary>
        [NotNull]
        public SynthSettings CreatePatch()
        {
            var patch = new SynthSettings();
            configure(patch);
            return patch;
        }

        public override string ToString() => Name;
    }

    /// <summary>
    /// Classic subtractive synth sounds to start from.
    /// </summary>
    public static class SynthPresets
    {
        [NotNull]
        [ItemNotNull]
        public static IReadOnlyList<SynthPreset> All { get; } = new[]
        {
            new SynthPreset("Init", "One bright sawtooth, filter open", synth => { }),

            new SynthPreset("Warm Pad", "Two detuned saws, slow and soft", synth =>
            {
                synth.Waveform = Waveform.Sawtooth;
                synth.Oscillator2Waveform = Waveform.Sawtooth;
                synth.Oscillator2Detune = 12;
                synth.OscillatorMix = 0.5;
                synth.Cutoff = 1400;
                synth.Resonance = 0.15;
                synth.KeyTracking = 0.5;
                synth.AttackSeconds = 0.6;
                synth.DecaySeconds = 0.5;
                synth.SustainLevel = 0.8;
                synth.ReleaseSeconds = 1.2;
                synth.LfoTarget = LfoTarget.Filter;
                synth.LfoRate = 0.3;
                synth.LfoDepth = 0.15;
            }),

            new SynthPreset("Acid Bass", "Resonant squelch on a sawtooth", synth =>
            {
                synth.Waveform = Waveform.Sawtooth;
                synth.Cutoff = 300;
                synth.Resonance = 0.8;
                synth.FilterEnvelopeAmount = 3.5;
                synth.FilterAttackSeconds = 0.001;
                synth.FilterDecaySeconds = 0.18;
                synth.FilterSustainLevel = 0;
                synth.FilterReleaseSeconds = 0.1;
                synth.AttackSeconds = 0.002;
                synth.DecaySeconds = 0.2;
                synth.SustainLevel = 0.6;
                synth.ReleaseSeconds = 0.08;
            }),

            new SynthPreset("Pluck", "Short and percussive, like a plucked string", synth =>
            {
                synth.Waveform = Waveform.Sawtooth;
                synth.Oscillator2Waveform = Waveform.Square;
                synth.Oscillator2Semitones = 12;
                synth.OscillatorMix = 0.25;
                synth.Cutoff = 500;
                synth.FilterEnvelopeAmount = 4;
                synth.FilterAttackSeconds = 0.001;
                synth.FilterDecaySeconds = 0.25;
                synth.FilterSustainLevel = 0;
                synth.KeyTracking = 0.7;
                synth.AttackSeconds = 0.002;
                synth.DecaySeconds = 0.4;
                synth.SustainLevel = 0;
                synth.ReleaseSeconds = 0.3;
            }),

            new SynthPreset("Brass", "Swelling filter, like a horn section", synth =>
            {
                synth.Waveform = Waveform.Sawtooth;
                synth.Oscillator2Waveform = Waveform.Sawtooth;
                synth.Oscillator2Detune = 7;
                synth.OscillatorMix = 0.5;
                synth.Cutoff = 700;
                synth.Resonance = 0.1;
                synth.FilterEnvelopeAmount = 2.5;
                synth.FilterAttackSeconds = 0.08;
                synth.FilterDecaySeconds = 0.4;
                synth.FilterSustainLevel = 0.6;
                synth.KeyTracking = 0.4;
                synth.AttackSeconds = 0.05;
                synth.SustainLevel = 0.9;
                synth.ReleaseSeconds = 0.25;
            }),

            new SynthPreset("Lead", "Square lead with vibrato", synth =>
            {
                synth.Waveform = Waveform.Square;
                synth.Oscillator2Waveform = Waveform.Sawtooth;
                synth.Oscillator2Semitones = 12;
                synth.OscillatorMix = 0.3;
                synth.Cutoff = 3500;
                synth.Resonance = 0.25;
                synth.KeyTracking = 0.5;
                synth.AttackSeconds = 0.01;
                synth.SustainLevel = 0.85;
                synth.ReleaseSeconds = 0.15;
                synth.LfoTarget = LfoTarget.Pitch;
                synth.LfoRate = 5.5;
                synth.LfoDepth = 0.25;
            }),

            new SynthPreset("Wobble Bass", "An LFO sweeping a resonant filter", synth =>
            {
                synth.Waveform = Waveform.Sawtooth;
                synth.Oscillator2Waveform = Waveform.Square;
                synth.Oscillator2Semitones = -12;
                synth.OscillatorMix = 0.5;
                synth.Cutoff = 350;
                synth.Resonance = 0.6;
                synth.AttackSeconds = 0.005;
                synth.SustainLevel = 1;
                synth.ReleaseSeconds = 0.1;
                synth.LfoTarget = LfoTarget.Filter;
                synth.LfoRate = 4;
                synth.LfoDepth = 0.7;
            }),

            new SynthPreset("Bell", "Bright, inharmonic and ringing", synth =>
            {
                synth.Waveform = Waveform.Sine;
                synth.Oscillator2Waveform = Waveform.Triangle;
                synth.Oscillator2Semitones = 19;
                synth.Oscillator2Detune = 14;
                synth.OscillatorMix = 0.4;
                synth.Cutoff = 6000;
                synth.FilterType = FilterType.LowPass;
                synth.AttackSeconds = 0.001;
                synth.DecaySeconds = 1.5;
                synth.SustainLevel = 0;
                synth.ReleaseSeconds = 1.5;
            })
        };
    }
}
