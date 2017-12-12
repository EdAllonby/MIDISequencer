using JetBrains.Annotations;
using Sequencer.Audio.Calculator;
using Sequencer.Domain;
using Sequencer.Domain.Settings;
using Sequencer.Visual;

namespace Sequencer.Audio
{
    public class SignalProviderFactory : ISignalProviderFactory
    {
        [NotNull] private readonly IFrequencyCalculator frequencyCalculator;
        private readonly int sampleRate;

        public SignalProviderFactory([NotNull] IAudioSettings audioSettings, [NotNull] IFrequencyCalculator frequencyCalculator)
        {
            sampleRate = audioSettings.SampleRate;
            this.frequencyCalculator = frequencyCalculator;
        }

        public ISignalProvider CreateSignalProvider(IVisualNote visualNote)
        {
            var signalProvider = new SignalProvider(new SawtoothWaveCalculator(19))
            {
                Amplitude = (float) visualNote.Velocity.Volume,
                Frequency = (float) frequencyCalculator.PitchFrequency(visualNote.Pitch)
            };

            signalProvider.SetWaveFormat(sampleRate, 1);

            return signalProvider;
        }
    }
}