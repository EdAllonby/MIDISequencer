using System;
using System.IO;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Sequencer.Domain.Arrangement;
using Sequencer.Domain.Effects;

namespace Sequencer.Domain.Tests
{
    [TestFixture]
    public class EffectTests
    {
        [Test]
        public void EveryEffect_StartsAtItsDefaults_WithinRange()
        {
            foreach (EffectKind kind in EffectCatalogue.All)
            {
                var effect = new Effect(kind);

                Assert.That(effect.Parameters, Is.Not.Empty, kind.ToString());
                Assert.That(effect.IsEnabled, Is.True);

                foreach (EffectParameter parameter in effect.Parameters)
                {
                    Assert.That(effect[parameter.Key], Is.EqualTo(parameter.Default));
                    Assert.That(parameter.Default, Is.InRange(parameter.Minimum, parameter.Maximum), $"{kind} {parameter.Name}");
                }
            }
        }

        [Test]
        public void Values_AreClampedToTheirRange()
        {
            var compressor = new Effect(EffectKind.Compressor);

            compressor["ratio"] = 500;
            compressor["threshold"] = double.NaN;

            Assert.That(compressor["ratio"], Is.EqualTo(20));
            Assert.That(compressor["threshold"], Is.EqualTo(-18), "NaN falls back to the default.");
        }

        [Test]
        public void Choices_SnapToAWholeChoice()
        {
            var delay = new Effect(EffectKind.Delay);

            delay["time"] = 3.4;

            Assert.That(delay["time"], Is.EqualTo(3));
            Assert.That(delay.Parameters.Single(p => p.Key == "time").Format(3), Is.EqualTo("1/4"));
        }

        [TestCase(ParameterUnit.Decibels, -18, "−18 dB")]
        [TestCase(ParameterUnit.Decibels, 6, "+6 dB")]
        [TestCase(ParameterUnit.Decibels, 0, "0 dB")]
        [TestCase(ParameterUnit.Percent, 0.25, "25 %")]
        [TestCase(ParameterUnit.Milliseconds, 0.5, "0.5 ms")]
        [TestCase(ParameterUnit.Milliseconds, 120, "120 ms")]
        [TestCase(ParameterUnit.Hertz, 1200, "1.2 kHz")]
        [TestCase(ParameterUnit.Hertz, 0.8, "0.8 Hz")]
        [TestCase(ParameterUnit.Ratio, 4, "4:1")]
        [TestCase(ParameterUnit.Ratio, 20, "∞:1")]
        public void Values_ReadNaturally(ParameterUnit unit, double value, string expected)
        {
            var parameter = new EffectParameter("x", "X", -1000, 20000, 0, unit);

            Assert.That(parameter.Format(value), Is.EqualTo(expected));
        }

        [Test]
        public void LogarithmicParameters_PutTheGeometricMiddleHalfwayAlongTheSlider()
        {
            EffectParameter release = new Effect(EffectKind.Compressor).Parameters.Single(p => p.Key == "release");

            Assert.That(release.ToPosition(100), Is.EqualTo(0.5).Within(1e-9));
            Assert.That(release.FromPosition(0.5), Is.EqualTo(100).Within(1e-9));
        }

        [Test]
        public void TheChain_CanBeAddedToReorderedAndRemovedFrom()
        {
            var track = new Track(TrackKind.Synth, "Synth", 0, 8);
            var changes = 0;
            track.EffectsChanged += (sender, args) => changes++;
            var drive = new Effect(EffectKind.Drive);
            var delay = new Effect(EffectKind.Delay);
            var reverb = new Effect(EffectKind.Reverb);

            track.AddEffect(drive);
            track.AddEffect(delay);
            track.AddEffect(reverb);
            track.MoveEffect(reverb, -1);
            Assert.That(track.Effects, Is.EqualTo(new[] { drive, reverb, delay }));

            track.RemoveEffect(drive);
            Assert.That(track.Effects, Is.EqualTo(new[] { reverb, delay }));

            delay["mix"] = 0.5;
            reverb.IsEnabled = false;
            Assert.That(changes, Is.EqualTo(7), "Every change to the chain or an effect is announced.");

            drive["drive"] = 20;
            Assert.That(changes, Is.EqualTo(7), "A removed effect no longer counts as the track's.");
        }

        [Test]
        public void Effects_AreSavedAndLoaded()
        {
            Song song = Song.CreateDefault();
            var compressor = new Effect(EffectKind.Compressor) { ["ratio"] = 8, ["makeup"] = 3 };
            var reverb = new Effect(EffectKind.Reverb) { IsEnabled = false };
            song.Tracks[1].AddEffect(compressor);
            song.Tracks[1].AddEffect(reverb);

            var stream = new MemoryStream();
            SongFile.Save(song, 120, stream);
            stream.Position = 0;
            Track drums = SongFile.Load(stream).Song.Tracks[1];

            Assert.That(drums.Effects.Select(e => e.Kind), Is.EqualTo(new[] { EffectKind.Compressor, EffectKind.Reverb }));
            Assert.That(drums.Effects[0]["ratio"], Is.EqualTo(8));
            Assert.That(drums.Effects[0]["makeup"], Is.EqualTo(3));
            Assert.That(drums.Effects[1].IsEnabled, Is.False);
            Assert.That(SongFile.Load(new MemoryStream(stream.ToArray())).Song.Tracks[0].Effects, Is.Empty);
        }

        [Test]
        public void UnknownParameters_InAFile_AreIgnored()
        {
            const string json = "{\"format\": \"sequencer-song\", \"version\": 1, \"tracks\": [{\"kind\": \"Synth\", \"effects\": " +
                                "[{\"kind\": \"Delay\", \"parameters\": {\"mix\": 0.6, \"wow\": 3}}]}]}";

            Track track = SongFile.Load(new MemoryStream(Encoding.UTF8.GetBytes(json))).Song.Tracks[0];

            Assert.That(track.Effects.Single()["mix"], Is.EqualTo(0.6));
            Assert.That(track.Effects.Single().IsEnabled, Is.True);
        }
    }
}
