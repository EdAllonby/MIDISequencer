using System;
using JetBrains.Annotations;
using NAudio.Dsp;
using NAudio.Wave;
using Sequencer.Audio.Calculator;
using Sequencer.Domain.Settings;

namespace Sequencer.Audio
{
    /// <summary>
    /// One note of the subtractive synth: two oscillators and noise, through a resonant state-variable filter
    /// with its own envelope, then the amp envelope, with an LFO for vibrato, filter sweeps or tremolo.
    /// The patch is copied when the note is created, so changing the synth mid-note can't tear its settings.
    /// </summary>
    public sealed class SubtractiveVoice : ISignalProvider
    {
        private const double MiddleCFrequency = 261.63;
        private const int ControlRate = 8;

        [NotNull] private readonly EnvelopeGenerator ampEnvelope = new EnvelopeGenerator();
        [NotNull] private readonly EnvelopeGenerator filterEnvelope = new EnvelopeGenerator();
        [NotNull] private readonly IWaveformCalculator oscillator1;
        [NotNull] private readonly IWaveformCalculator oscillator2;
        private readonly int sampleRate;

        // The patch, copied at note start.
        private readonly double oscillator2Ratio;
        private readonly float oscillatorMix;
        private readonly float noiseLevel;
        private readonly FilterType filterType;
        private readonly double baseCutoff;
        private readonly double damping;
        private readonly double filterEnvelopeAmount;
        private readonly double lfoIncrement;
        private readonly double lfoDepth;
        private readonly LfoTarget lfoTarget;

        private float amplitude;
        private float frequency;
        private double phase1;
        private double phase2;
        private double lfoPhase;
        private uint noiseState = 0x2545F491;

        // State-variable filter: integrator state and the coefficients for the current cutoff.
        private double integrator1;
        private double integrator2;
        private double a1;
        private double a2;
        private double a3;
        private int controlCounter;

        private volatile bool releaseRequested;
        private bool released;
        private volatile bool finished;
        private int autoReleaseSamplesRemaining = -1;

        public SubtractiveVoice([NotNull] SynthSettings patch, [NotNull] Func<Waveform, IWaveformCalculator> waveforms)
        {
            sampleRate = patch.SampleRate;
            WaveFormat = WaveFormat.CreateIeeeFloatWaveFormat(sampleRate, 1);
            oscillator1 = waveforms(patch.Waveform);
            oscillator2 = waveforms(patch.Oscillator2Waveform);
            oscillator2Ratio = Math.Pow(2, (patch.Oscillator2Semitones + patch.Oscillator2Detune / 100.0) / 12.0);
            oscillatorMix = (float) patch.OscillatorMix;
            noiseLevel = (float) patch.NoiseLevel;
            filterType = patch.FilterType;
            baseCutoff = patch.Cutoff;
            KeyTracking = patch.KeyTracking;

            // Resonance 0..1 maps to Q from 0.5 to about 20; the filter uses damping, 1/Q.
            damping = 2.0 * (1.0 - patch.Resonance * 0.975);
            filterEnvelopeAmount = patch.FilterEnvelopeAmount;
            lfoIncrement = patch.LfoRate / sampleRate;
            lfoDepth = patch.LfoDepth;
            lfoTarget = patch.LfoTarget;

            ampEnvelope.AttackRate = (float) (patch.AttackSeconds * sampleRate);
            ampEnvelope.DecayRate = (float) (patch.DecaySeconds * sampleRate);
            ampEnvelope.SustainLevel = (float) patch.SustainLevel;
            ampEnvelope.ReleaseRate = (float) (patch.ReleaseSeconds * sampleRate);

            filterEnvelope.AttackRate = (float) (patch.FilterAttackSeconds * sampleRate);
            filterEnvelope.DecayRate = (float) (patch.FilterDecaySeconds * sampleRate);
            filterEnvelope.SustainLevel = (float) patch.FilterSustainLevel;
            filterEnvelope.ReleaseRate = (float) (patch.FilterReleaseSeconds * sampleRate);

            Frequency = 440;
            Amplitude = 0.25f;
        }

        public WaveFormat WaveFormat { get; }

        public bool IsFinished => finished;

        private double KeyTracking { get; }

        public float Amplitude
        {
            get => amplitude;
            set
            {
                if (value >= 0 && value <= 1)
                {
                    amplitude = value;
                }
            }
        }

        public float Frequency
        {
            get => frequency;
            set
            {
                if (value > 0 && value < 20000)
                {
                    frequency = value;
                }
            }
        }

        public void NoteOn()
        {
            ampEnvelope.Gate(true);
            filterEnvelope.Gate(true);
        }

        public void NoteOff()
        {
            releaseRequested = true;
        }

        public void ReleaseAfter(int sampleCount)
        {
            autoReleaseSamplesRemaining = sampleCount;
        }

        public int Read(Span<float> buffer)
        {
            if (finished)
            {
                return 0;
            }

            if (autoReleaseSamplesRemaining >= 0)
            {
                autoReleaseSamplesRemaining -= buffer.Length;

                if (autoReleaseSamplesRemaining <= 0)
                {
                    autoReleaseSamplesRemaining = -1;
                    releaseRequested = true;
                }
            }

            if (releaseRequested && !released)
            {
                ampEnvelope.Gate(false);
                filterEnvelope.Gate(false);
                released = true;
            }

            for (var n = 0; n < buffer.Length; n++)
            {
                double lfo = Math.Sin(2 * Math.PI * lfoPhase) * lfoDepth;
                lfoPhase += lfoIncrement;

                if (lfoPhase >= 1)
                {
                    lfoPhase -= 1;
                }

                // Oscillators. Vibrato bends both by up to a semitone.
                double pitch = lfoTarget == LfoTarget.Pitch ? Math.Pow(2, lfo / 12.0) : 1.0;
                double step1 = frequency * pitch / sampleRate;
                double step2 = step1 * oscillator2Ratio;
                double oscillators = oscillator1.CalculateForPhase(phase1, step1 * sampleRate) * (1 - oscillatorMix) +
                                     oscillator2.CalculateForPhase(phase2, step2 * sampleRate) * oscillatorMix;
                double input = oscillators + Noise() * noiseLevel;
                phase1 = Advance(phase1, step1);
                phase2 = Advance(phase2, step2);

                // Filter, with the cutoff recalculated every few samples.
                float filterLevel = filterEnvelope.Process();

                if (controlCounter-- <= 0)
                {
                    controlCounter = ControlRate - 1;
                    UpdateFilter(filterLevel, lfoTarget == LfoTarget.Filter ? lfo : 0);
                }

                double filtered = Filter(input);

                // Amp envelope, with tremolo dipping the level.
                double tremolo = lfoTarget == LfoTarget.Volume ? 1 - lfoDepth * 0.5 * (1 - Math.Sin(2 * Math.PI * lfoPhase)) : 1.0;
                buffer[n] = (float) (filtered * ampEnvelope.Process() * amplitude * tremolo);
            }

            if (released && ampEnvelope.State == EnvelopeGenerator.EnvelopeState.Idle)
            {
                finished = true;
            }

            return buffer.Length;
        }

        private static double Advance(double phase, double step)
        {
            phase += step;
            return phase >= 1 ? phase - Math.Floor(phase) : phase;
        }

        /// <summary>
        /// Sets the filter's coefficients for a cutoff moved by the envelope, the note (key tracking) and the LFO.
        /// </summary>
        private void UpdateFilter(float envelopeLevel, double lfo)
        {
            double octaves = filterEnvelopeAmount * envelopeLevel + lfo * 2 + KeyTracking * Math.Log(frequency / MiddleCFrequency, 2);
            double cutoff = Math.Clamp(baseCutoff * Math.Pow(2, octaves), SynthSettings.MinimumCutoff, sampleRate * 0.45);

            // Zero-delay-feedback state-variable filter (Zavalishin): stable even with fast sweeps and high resonance.
            double g = Math.Tan(Math.PI * cutoff / sampleRate);
            a1 = 1 / (1 + g * (g + damping));
            a2 = g * a1;
            a3 = g * a2;
        }

        private double Filter(double input)
        {
            double v3 = input - integrator2;
            double v1 = a1 * integrator1 + a2 * v3;
            double v2 = integrator2 + a2 * integrator1 + a3 * v3;
            integrator1 = 2 * v1 - integrator1;
            integrator2 = 2 * v2 - integrator2;

            switch (filterType)
            {
                case FilterType.HighPass:
                    return input - damping * v1 - v2;
                case FilterType.BandPass:
                    return v1 * damping;
                default:
                    return v2;
            }
        }

        private float Noise()
        {
            noiseState ^= noiseState << 13;
            noiseState ^= noiseState >> 17;
            noiseState ^= noiseState << 5;
            return noiseState / (float) uint.MaxValue * 2f - 1f;
        }
    }
}
