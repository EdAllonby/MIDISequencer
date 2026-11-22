using System;
using JetBrains.Annotations;
using NAudio.Wave;
using Sequencer.Domain.Settings;

namespace Sequencer.Audio
{
    /// <summary>
    /// Scales the mixed output by the current master volume, read on every buffer so changes apply immediately.
    /// </summary>
    public sealed class MasterVolumeSampleProvider : ISampleProvider
    {
        [NotNull] private readonly IAudioSettings audioSettings;
        [NotNull] private readonly ISampleProvider source;

        public MasterVolumeSampleProvider([NotNull] ISampleProvider source, [NotNull] IAudioSettings audioSettings)
        {
            this.source = source;
            this.audioSettings = audioSettings;
        }

        public WaveFormat WaveFormat => source.WaveFormat;

        public int Read(Span<float> buffer)
        {
            int read = source.Read(buffer);
            var volume = (float) audioSettings.MasterVolume;

            for (var n = 0; n < read; n++)
            {
                buffer[n] *= volume;
            }

            return read;
        }
    }
}
