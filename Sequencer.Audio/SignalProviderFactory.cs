using JetBrains.Annotations;
using Sequencer.Audio.Calculator;
using Sequencer.Domain;
using Sequencer.Domain.Settings;

namespace Sequencer.Audio
{
    public class SignalProviderFactory : ISignalProviderFactory
    {
        [NotNull] private readonly IAudioSettings audioSettings;
        [NotNull] private readonly IFrequencyCalculator frequencyCalculator;

        // Building the sawtooth from 19 harmonics is expensive, so it is computed once into a wavetable
        // that every voice shares.
        [NotNull] private readonly IWaveformCalculator waveform = new WavetableCalculator(new SawtoothWaveCalculator(19));

        public SignalProviderFactory([NotNull] IAudioSettings audioSettings, [NotNull] IFrequencyCalculator frequencyCalculator)
        {
            this.audioSettings = audioSettings;
            this.frequencyCalculator = frequencyCalculator;
        }

        public ISignalProvider CreateSignalProvider(ISequencedNote note)
        {
            return new SignalProvider(waveform, audioSettings)
            {
                Amplitude = (float) note.Tone.Velocity.Volume,
                Frequency = (float) frequencyCalculator.PitchFrequency(note.Tone.Pitch)
            };
        }
    }
}
