using JetBrains.Annotations;
using Sequencer.Audio.Calculator;
using Sequencer.Domain;
using Sequencer.Visual;

namespace Sequencer.Audio
{
    public class SignalProviderFactory : ISignalProviderFactory
    {
        [NotNull] private readonly IFrequencyCalculator frequencyCalculator;

        public SignalProviderFactory([NotNull] IFrequencyCalculator frequencyCalculator)
        {
            this.frequencyCalculator = frequencyCalculator;
        }

        public ISignalProvider CreateSignalProvider(IVisualNote visualNote)
        {
            var signalProvider = new SignalProvider(new SawtoothWaveCalculator(9))
            {
                Amplitude = (float) visualNote.Velocity.Volume,
                Frequency = (float) frequencyCalculator.PitchFrequency(visualNote.Pitch)
            };

            signalProvider.SetWaveFormat(44100, 1);

            return signalProvider;
        }
    }
}