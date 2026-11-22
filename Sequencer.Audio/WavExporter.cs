using System;
using JetBrains.Annotations;
using NAudio.Wave;
using Sequencer.Domain.Arrangement;
using Sequencer.Domain.Settings;

namespace Sequencer.Audio
{
    /// <summary>
    /// Renders the song to a WAV file (16-bit, 44.1 kHz, mono): the whole arrangement in song mode, with a
    /// second at the end for the last notes to fade.
    /// </summary>
    public sealed class WavExporter
    {
        [NotNull] private readonly OfflineRenderer renderer;

        public WavExporter([NotNull] OfflineRenderer renderer)
        {
            this.renderer = renderer;
        }

        /// <summary>
        /// Returns the length written, in seconds.
        /// </summary>
        public double Export([NotNull] Song song, int beatsPerMinute, [NotNull] MixerSettings mixer, [NotNull] string path)
        {
            float[] samples = renderer.Render(song, beatsPerMinute, mixer);

            using (var writer = new WaveFileWriter(path, new WaveFormat(mixer.SampleRate, 16, 1)))
            {
                foreach (float sample in samples)
                {
                    writer.WriteSample(Math.Clamp(sample, -1f, 1f));
                }
            }

            return samples.Length / (double) mixer.SampleRate;
        }
    }
}
