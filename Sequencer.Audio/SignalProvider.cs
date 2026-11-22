using JetBrains.Annotations;
using NAudio.Dsp;
using NAudio.Wave;
using Sequencer.Audio.Calculator;
using Sequencer.Domain.Settings;

namespace Sequencer.Audio
{
    public sealed class SignalProvider : ISignalProvider
    {
        [NotNull] private readonly EnvelopeGenerator envelope = new EnvelopeGenerator();
        [NotNull] private readonly IWaveformCalculator waveformCalculator;
        private readonly int sampleRate;
        private float amplitude;
        private float frequency;

        // Position within the current cycle, 0 to 1. Carried across buffers so there is never a phase jump.
        private double phase;

        // NoteOff can be called from any thread; the audio thread applies it at the start of its next read.
        private volatile bool releaseRequested;
        private bool released;
        private volatile bool finished;

        // Samples left before an automatic release, or -1 for none.
        private int autoReleaseSamplesRemaining = -1;

        public SignalProvider([NotNull] IWaveformCalculator waveformCalculator, [NotNull] IAudioSettings audioSettings)
        {
            this.waveformCalculator = waveformCalculator;
            sampleRate = audioSettings.SampleRate;
            WaveFormat = WaveFormat.CreateIeeeFloatWaveFormat(sampleRate, 1);

            envelope.AttackRate = (float) (audioSettings.AttackSeconds * sampleRate);
            envelope.DecayRate = (float) (audioSettings.DecaySeconds * sampleRate);
            envelope.SustainLevel = (float) audioSettings.SustainLevel;
            envelope.ReleaseRate = (float) (audioSettings.ReleaseSeconds * sampleRate);

            Frequency = 1000f;
            Amplitude = 0.25f;
        }

        public WaveFormat WaveFormat { get; }

        public bool IsFinished => finished;

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
            envelope.Gate(true);
        }

        public void NoteOff()
        {
            releaseRequested = true;
        }

        public void ReleaseAfter(int sampleCount)
        {
            autoReleaseSamplesRemaining = sampleCount;
        }

        public int Read(System.Span<float> buffer)
        {
            if (finished)
            {
                // Returning fewer samples than requested tells the mixer to drop this voice.
                return 0;
            }

            if (autoReleaseSamplesRemaining >= 0)
            {
                // Counted per buffer, so the release can start up to one buffer (a few ms) early.
                autoReleaseSamplesRemaining -= buffer.Length;

                if (autoReleaseSamplesRemaining <= 0)
                {
                    autoReleaseSamplesRemaining = -1;
                    releaseRequested = true;
                }
            }

            if (releaseRequested && !released)
            {
                envelope.Gate(false);
                released = true;
            }

            double phaseStep = (double) Frequency / sampleRate;

            for (var n = 0; n < buffer.Length; n++)
            {
                float envelopeAmplitude = envelope.Process();
                buffer[n] = Amplitude * envelopeAmplitude * waveformCalculator.CalculateForPhase(phase);

                phase += phaseStep;

                if (phase >= 1.0)
                {
                    phase -= 1.0;
                }
            }

            if (released && envelope.State == EnvelopeGenerator.EnvelopeState.Idle)
            {
                finished = true;
            }

            return buffer.Length;
        }
    }
}
