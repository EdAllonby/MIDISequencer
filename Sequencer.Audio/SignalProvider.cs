using JetBrains.Annotations;
using NAudio.Dsp;
using NAudio.Wave;
using Sequencer.Audio.Calculator;

namespace Sequencer.Audio
{
    public sealed class SignalProvider : WaveProvider32, ISignalProvider
    {
        [NotNull] private readonly EnvelopeGenerator envelope = new EnvelopeGenerator();
        [NotNull] private readonly IWaveformCalculator waveformCalculator;
        private float amplitude;
        private float frequency;
        private int sample;

        public SignalProvider([NotNull] IWaveformCalculator waveformCalculator)
        {
            envelope.AttackRate = 44100;
            envelope.DecayRate = 44100;
            envelope.SustainLevel = 0.5f;
            envelope.ReleaseRate = 44100;

            this.waveformCalculator = waveformCalculator;

            Frequency = 1000f;
            Amplitude = 0.25f;
        }

        public bool NoteState
        {
            get => envelope.State != EnvelopeGenerator.EnvelopeState.Idle;
            set => envelope.Gate(value);
        }

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

        public override int Read([NotNull] float[] buffer, int offset, int sampleCount)
        {
            if (WaveFormat != null)
            {
                int sampleRate = WaveFormat.SampleRate;

                for (var n = 0; n < sampleCount; n++)
                {
                    float envelopeAmplitude = envelope.Process();

                    if (envelopeAmplitude > 0)
                    {
                        buffer[n + offset] = Amplitude * envelopeAmplitude * waveformCalculator.CalculateForSample(sample, Frequency, sampleRate);
                    }
                    else
                    {
                        buffer[n + offset] = 0;
                    }
                    sample++;

                    if (sample >= sampleRate)
                    {
                        sample = 0;
                    }
                }
            }

            return sampleCount;
        }
    }
}