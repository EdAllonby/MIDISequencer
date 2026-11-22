using System.Collections.Generic;
using JetBrains.Annotations;
using Sequencer.Audio.Calculator;
using Sequencer.Domain;
using Sequencer.Domain.Settings;

namespace Sequencer.Audio
{
    public class SignalProviderFactory : ISignalProviderFactory
    {
        [NotNull] private readonly IFrequencyCalculator frequencyCalculator;

        // Band-limited wavetables, one per octave per waveform, built once and shared by every voice.
        [NotNull] private readonly IReadOnlyDictionary<Waveform, IWaveformCalculator> waveforms = BandLimitedWavetable.Standard;

        public SignalProviderFactory([NotNull] IFrequencyCalculator frequencyCalculator)
        {
            this.frequencyCalculator = frequencyCalculator;
        }

        [NotNull]
        private IWaveformCalculator CalculatorFor(Waveform waveform) =>
            waveforms.TryGetValue(waveform, out IWaveformCalculator calculator) ? calculator : waveforms[Waveform.Sawtooth];

        public ISignalProvider CreateSignalProvider(ISequencedNote note, IAudioSettings instrument)
        {
            if (instrument is SynthSettings patch)
            {
                // The full subtractive synth: oscillators, filter, envelopes and LFO.
                return new SubtractiveVoice(patch, CalculatorFor)
                {
                    Amplitude = (float) (note.Tone.Velocity.Volume * patch.Volume) * MixLevels.SynthVoice,
                    Frequency = (float) frequencyCalculator.PitchFrequency(note.Tone.Pitch)
                };
            }

            if (!waveforms.TryGetValue(instrument.Waveform, out IWaveformCalculator waveform))
            {
                waveform = waveforms[Waveform.Sawtooth];
            }

            return new SignalProvider(waveform, instrument)
            {
                Amplitude = (float) (note.Tone.Velocity.Volume * instrument.Volume) * MixLevels.SynthVoice,
                Frequency = (float) frequencyCalculator.PitchFrequency(note.Tone.Pitch)
            };
        }
    }
}
