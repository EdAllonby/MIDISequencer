using System;
using System.Collections.Generic;
using JetBrains.Annotations;

namespace Sequencer.Audio.Calculator
{
    /// <summary>
    /// A waveform stored as one table per octave, each with only the harmonics that stay below the Nyquist
    /// frequency (half the sample rate) for notes in that octave. A single table with a fixed number of
    /// harmonics either sounds dull on low notes or aliases on high ones: harmonics above Nyquist fold back as
    /// inharmonic, gritty tones.
    /// </summary>
    public sealed class BandLimitedWavetable : IWaveformCalculator
    {
        private const double LowestFrequency = 20;
        private const int OctaveCount = 11;

        // Linear interpolation needs several table points per cycle of the highest harmonic to stay clean.
        private const int MaximumHarmonics = 128;

        [NotNull] [ItemNotNull] private readonly WavetableCalculator[] octaves = new WavetableCalculator[OctaveCount];

        /// <param name="createWaveform">Builds the waveform with a given number of harmonics.</param>
        /// <param name="sampleRate">The output sample rate, which sets the Nyquist frequency.</param>
        public BandLimitedWavetable([NotNull] Func<int, IWaveformCalculator> createWaveform, int sampleRate = 44100)
        {
            for (var octave = 0; octave < OctaveCount; octave++)
            {
                // Every note in this octave is at most this frequency, so its harmonics up to here are safe.
                double highestNote = TopOfOctave(octave);
                int harmonics = Math.Clamp((int) Math.Floor(sampleRate * 0.5 / highestNote), 1, MaximumHarmonics);
                octaves[octave] = new WavetableCalculator(createWaveform(harmonics));
            }
        }

        /// <summary>
        /// The table with the most harmonics, for callers that don't know the note.
        /// </summary>
        public float CalculateForPhase(double phase) => octaves[0].CalculateForPhase(phase);

        public float CalculateForPhase(double phase, double frequency) => octaves[OctaveFor(frequency)].CalculateForPhase(phase);

        private static double TopOfOctave(int octave) => LowestFrequency * Math.Pow(2, octave + 1);

        private static int OctaveFor(double frequency)
        {
            for (var octave = 0; octave < OctaveCount; octave++)
            {
                if (frequency <= TopOfOctave(octave))
                {
                    return octave;
                }
            }

            return OctaveCount - 1;
        }

        /// <summary>
        /// The shared band-limited tables for each waveform, built once per process (about a quarter of a second).
        /// </summary>
        [NotNull]
        public static IReadOnlyDictionary<Domain.Settings.Waveform, IWaveformCalculator> Standard => StandardTables.Value;

        [NotNull] private static readonly Lazy<IReadOnlyDictionary<Domain.Settings.Waveform, IWaveformCalculator>> StandardTables =
            new Lazy<IReadOnlyDictionary<Domain.Settings.Waveform, IWaveformCalculator>>(() =>
                new Dictionary<Domain.Settings.Waveform, IWaveformCalculator>
                {
                    { Domain.Settings.Waveform.Sawtooth, new BandLimitedWavetable(harmonics => new SawtoothWaveCalculator(harmonics)) },
                    { Domain.Settings.Waveform.Square, new BandLimitedWavetable(harmonics => new SquareWaveCalculator(harmonics)) },
                    { Domain.Settings.Waveform.Triangle, new BandLimitedWavetable(harmonics => new TriangleWaveCalculator(harmonics)) },
                    { Domain.Settings.Waveform.Sine, new SineWaveCalculator() }
                });
    }
}
