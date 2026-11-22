using System;
using JetBrains.Annotations;
using NAudio.Wave;
using Sequencer.Domain.Settings;

namespace Sequencer.Audio
{
    /// <summary>
    /// Scales the mixed output by the master volume. The volume is read on every buffer, and glides to a new
    /// value over a few milliseconds so moving the slider does not cause zipper clicks.
    /// </summary>
    public sealed class MasterVolumeSampleProvider : ISampleProvider
    {
        [NotNull] private readonly IAudioSettings audioSettings;
        [NotNull] private readonly ISampleProvider source;
        private readonly float glideCoefficient;
        private float currentVolume = float.NaN;

        public MasterVolumeSampleProvider([NotNull] ISampleProvider source, [NotNull] IAudioSettings audioSettings)
        {
            this.source = source;
            this.audioSettings = audioSettings;

            // About 5 ms to settle.
            glideCoefficient = (float) Math.Exp(-1.0 / (0.005 * source.WaveFormat.SampleRate));
        }

        public WaveFormat WaveFormat => source.WaveFormat;

        public int Read(Span<float> buffer)
        {
            int read = source.Read(buffer);
            var target = (float) audioSettings.MasterVolume;

            if (float.IsNaN(currentVolume))
            {
                currentVolume = target;
            }

            for (var n = 0; n < read; n++)
            {
                currentVolume = glideCoefficient * currentVolume + (1 - glideCoefficient) * target;
                buffer[n] *= currentVolume;
            }

            return read;
        }
    }
}
