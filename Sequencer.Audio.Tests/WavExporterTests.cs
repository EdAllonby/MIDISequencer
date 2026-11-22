using System;
using System.IO;
using NAudio.Wave;
using NUnit.Framework;
using Sequencer.Domain;
using Sequencer.Domain.Arrangement;
using Sequencer.Domain.Settings;
using Sequencer.Midi;

namespace Sequencer.Audio.Tests
{
    [TestFixture]
    public class WavExporterTests
    {
        [Test]
        public void Export_WritesTheWholeSongPlusATail()
        {
            Song song = Song.CreateDefault();
            song.Place(song.Tracks[1], 1, song.Tracks[1].ClipAt(0));
            var settings = new SequencerSettings();
            var exporter = new WavExporter(new OfflineRenderer(new FrequencyCalculator(new PitchAndPositionCalculator(settings)), new TickCalculator(settings)));
            string path = Path.Combine(Path.GetTempPath(), $"sequencer-test-{Guid.NewGuid():N}.wav");

            try
            {
                double seconds = exporter.Export(song, 120, new MixerSettings(), path);

                // Two sections of four bars at 120 BPM is 16 s, plus a second's tail.
                Assert.That(seconds, Is.EqualTo(17).Within(0.01));

                using (var reader = new WaveFileReader(path))
                {
                    Assert.That(reader.WaveFormat.SampleRate, Is.EqualTo(44100));
                    Assert.That(reader.WaveFormat.BitsPerSample, Is.EqualTo(16));
                    Assert.That(reader.TotalTime.TotalSeconds, Is.EqualTo(17).Within(0.01));
                }
            }
            finally
            {
                File.Delete(path);
            }
        }
    }
}
