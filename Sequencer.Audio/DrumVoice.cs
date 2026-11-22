using System;
using NAudio.Wave;
using Sequencer.Domain.Drums;

namespace Sequencer.Audio
{
    /// <summary>
    /// One synthesised drum hit. It plays once, then reports finished so the mixer drops it.
    /// </summary>
    public sealed class DrumVoice : ISampleProvider
    {
        private const double ChokeSeconds = 0.006;

        // A hit fades in over a millisecond (half that for hats) so it doesn't start with a click, and fades
        // to silence over its last 30 ms so it doesn't end with one.
        private const double FadeInSeconds = 0.001;
        private const double FadeOutSeconds = 0.03;

        // The six square-wave frequencies of the TR-808's cymbal circuit: an inharmonic, metallic cluster.
        private static readonly double[] MetallicFrequencies = { 205.3, 304.4, 369.6, 522.7, 540.0, 800.0 };

        private readonly DrumSound sound;
        private readonly float amplitude;
        private readonly int sampleRate;
        private readonly double lengthSeconds;
        private readonly double[] metallicPhases = new double[MetallicFrequencies.Length];

        private int sampleIndex;
        private double phase;
        private uint noiseState = 0x9E3779B9;

        // Filter state.
        private float highPassInput;
        private float highPassOutput;
        private float highPassOutput2;
        private float highPassInput2;
        private float bandLow;
        private float bandBand;

        private volatile bool chokeRequested;
        private int chokeStartSample = -1;

        public DrumVoice(DrumSound sound, byte velocity, int sampleRate, float level = 1f)
        {
            this.sound = sound;
            this.sampleRate = sampleRate;
            amplitude = velocity / 127f * level;
            WaveFormat = WaveFormat.CreateIeeeFloatWaveFormat(sampleRate, 1);
            lengthSeconds = LengthOf(sound);
        }

        public WaveFormat WaveFormat { get; }

        public DrumSound Sound => sound;

        public bool IsFinished { get; private set; }

        /// <summary>
        /// Fades the hit out within a few milliseconds, as a closed hi-hat cuts off an open one.
        /// Safe to call from any thread.
        /// </summary>
        public void Choke()
        {
            chokeRequested = true;
        }

        /// <summary>
        /// The fade-in and fade-out around the hit: 0 at the very start and end, 1 in between.
        /// </summary>
        private double Fade(double t)
        {
            double fadeIn = sound == DrumSound.ClosedHat || sound == DrumSound.OpenHat ? FadeInSeconds / 2 : FadeInSeconds;
            double rise = Math.Min(1, t / fadeIn);
            double untilEnd = lengthSeconds - t;
            double fall = untilEnd >= FadeOutSeconds ? 1 : 0.5 - 0.5 * Math.Cos(Math.PI * Math.Max(0, untilEnd) / FadeOutSeconds);
            return rise * fall;
        }

        public static double LengthOf(DrumSound sound)
        {
            switch (sound)
            {
                case DrumSound.Kick: return 0.5;
                case DrumSound.Snare: return 0.35;
                case DrumSound.Clap: return 0.4;
                case DrumSound.ClosedHat: return 0.12;
                case DrumSound.OpenHat: return 0.7;
                case DrumSound.Tom: return 0.6;
                default: return 0.3;
            }
        }

        public int Read(Span<float> buffer)
        {
            if (IsFinished)
            {
                return 0;
            }

            if (chokeRequested && chokeStartSample < 0)
            {
                chokeStartSample = sampleIndex;
            }

            for (var n = 0; n < buffer.Length; n++)
            {
                double t = (double) sampleIndex / sampleRate;
                float sample = t < lengthSeconds ? amplitude * Synthesise(t) * (float) Fade(t) : 0f;

                if (chokeStartSample >= 0)
                {
                    double sinceChoke = (double) (sampleIndex - chokeStartSample) / sampleRate;
                    sample *= (float) Math.Max(0, 1 - sinceChoke / ChokeSeconds);
                }

                buffer[n] = sample;
                sampleIndex++;
            }

            double elapsed = (double) sampleIndex / sampleRate;
            bool choked = chokeStartSample >= 0 && (double) (sampleIndex - chokeStartSample) / sampleRate >= ChokeSeconds;

            if (elapsed >= lengthSeconds || choked)
            {
                IsFinished = true;
            }

            return buffer.Length;
        }

        private float Synthesise(double t)
        {
            switch (sound)
            {
                case DrumSound.Kick:
                {
                    // A sine that drops quickly from ~155 Hz to 45 Hz, with a short click on the attack.
                    double frequency = 45 + 110 * Math.Exp(-t / 0.035);
                    double body = Oscillator(frequency) * Math.Exp(-t / 0.16);
                    double click = Noise() * Math.Exp(-t / 0.002) * 0.25;
                    return (float) (0.95 * body + click);
                }

                case DrumSound.Snare:
                {
                    double tone = Oscillator(185) * Math.Exp(-t / 0.06) * 0.45;
                    double rattle = HighPass(Noise(), 1500) * Math.Exp(-t / 0.13) * 0.7;
                    return (float) (tone + rattle);
                }

                case DrumSound.Clap:
                {
                    // Three quick bursts (several hands), then a short room tail, all band-passed.
                    double envelope = 0;

                    for (var burst = 0; burst < 3; burst++)
                    {
                        double since = t - burst * 0.011;

                        if (since >= 0)
                        {
                            envelope += Math.Exp(-since / 0.005);
                        }
                    }

                    if (t > 0.03)
                    {
                        envelope += 0.6 * Math.Exp(-(t - 0.03) / 0.11);
                    }

                    return (float) (BandPass(Noise(), 1150, 1.4) * envelope * 0.9);
                }

                case DrumSound.ClosedHat:
                    // The high-passing leaves little energy, so hats need a lot of gain to sit at ~0.4.
                    return (float) (Hat() * Math.Exp(-t / 0.03) * 2.5);

                case DrumSound.OpenHat:
                    return (float) (Hat() * Math.Exp(-t / 0.22) * 2.6);

                case DrumSound.Tom:
                {
                    double frequency = 92 + 60 * Math.Exp(-t / 0.08);
                    return (float) (Oscillator(frequency) * Math.Exp(-t / 0.2) * 0.85);
                }

                default:
                    return 0f;
            }
        }

        private double Oscillator(double frequency)
        {
            phase += frequency / sampleRate;

            if (phase >= 1)
            {
                phase -= 1;
            }

            return Math.Sin(2 * Math.PI * phase);
        }

        /// <summary>
        /// The metallic square cluster mixed with noise, then high-passed twice so only the sizzle is left.
        /// </summary>
        private double Hat()
        {
            double squares = 0;

            for (var i = 0; i < MetallicFrequencies.Length; i++)
            {
                metallicPhases[i] += MetallicFrequencies[i] / sampleRate;

                if (metallicPhases[i] >= 1)
                {
                    metallicPhases[i] -= 1;
                }

                squares += metallicPhases[i] < 0.5 ? 1 : -1;
            }

            double mixed = squares / MetallicFrequencies.Length * 0.6 + Noise() * 0.5;
            float once = HighPass((float) mixed, 6500);
            return HighPass2(once, 6500);
        }

        /// <summary>
        /// White noise from a fast xorshift generator: no allocation, and the same every time.
        /// </summary>
        private float Noise()
        {
            noiseState ^= noiseState << 13;
            noiseState ^= noiseState >> 17;
            noiseState ^= noiseState << 5;
            return noiseState / (float) uint.MaxValue * 2f - 1f;
        }

        private float HighPass(double input, double cutoff)
        {
            var coefficient = (float) Math.Exp(-2 * Math.PI * cutoff / sampleRate);
            highPassOutput = coefficient * (highPassOutput + (float) input - highPassInput);
            highPassInput = (float) input;
            return highPassOutput;
        }

        private float HighPass2(float input, double cutoff)
        {
            var coefficient = (float) Math.Exp(-2 * Math.PI * cutoff / sampleRate);
            highPassOutput2 = coefficient * (highPassOutput2 + input - highPassInput2);
            highPassInput2 = input;
            return highPassOutput2;
        }

        /// <summary>
        /// A state-variable band-pass filter.
        /// </summary>
        private float BandPass(float input, double centre, double q)
        {
            var f = (float) (2 * Math.Sin(Math.PI * centre / sampleRate));
            var damping = (float) (1 / q);
            bandLow += f * bandBand;
            float high = input - bandLow - damping * bandBand;
            bandBand += f * high;
            return bandBand;
        }
    }
}
