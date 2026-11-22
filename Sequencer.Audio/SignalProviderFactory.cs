using System.Collections.Generic;
using JetBrains.Annotations;
using Sequencer.Audio.Calculator;
using Sequencer.Domain;
using Sequencer.Domain.Settings;

namespace Sequencer.Audio
{
    public class SignalProviderFactory : ISignalProviderFactory
    {
        private const int Harmonics = 19;

        [NotNull] private readonly IFrequencyCalculator frequencyCalculator;

        // Adding up harmonics per sample is expensive, so each waveform is computed once into a wavetable
        // that every voice shares. Sine is cheap enough to calculate directly.
        [NotNull] private readonly Dictionary<Waveform, IWaveformCalculator> waveforms = new Dictionary<Waveform, IWaveformCalculator>
        {
            { Waveform.Sawtooth, new WavetableCalculator(new SawtoothWaveCalculator(Harmonics)) },
            { Waveform.Square, new WavetableCalculator(new SquareWaveCalculator(Harmonics)) },
            { Waveform.Triangle, new WavetableCalculator(new TriangleWaveCalculator(Harmonics)) },
            { Waveform.Sine, new SineWaveCalculator() }
        };

        public SignalProviderFactory([NotNull] IFrequencyCalculator frequencyCalculator)
        {
            this.frequencyCalculator = frequencyCalculator;
        }

        public ISignalProvider CreateSignalProvider(ISequencedNote note, IAudioSettings instrument)
        {
            if (!waveforms.TryGetValue(instrument.Waveform, out IWaveformCalculator waveform))
            {
                waveform = waveforms[Waveform.Sawtooth];
            }

            return new SignalProvider(waveform, instrument)
            {
                Amplitude = (float) (note.Tone.Velocity.Volume * instrument.Volume),
                Frequency = (float) frequencyCalculator.PitchFrequency(note.Tone.Pitch)
            };
        }
    }
}
