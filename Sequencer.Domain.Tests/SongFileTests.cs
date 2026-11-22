using System;
using System.IO;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Sequencer.Domain.Arrangement;
using Sequencer.Domain.Drums;
using Sequencer.Domain.Settings;

namespace Sequencer.Domain.Tests
{
    [TestFixture]
    public class SongFileTests
    {
        private static Tone ToneAt(Note note, int octave, int startTick, int lengthTicks, int velocity = 100) =>
            new Tone(new Pitch(note, octave), new Velocity(velocity), Position.PositionFromTick(startTick, TimeSignature.FourFour, 96),
                Position.PositionFromTick(startTick + lengthTicks, TimeSignature.FourFour, 96));

        private static LoadedSong RoundTrip(Song song, int bpm)
        {
            var stream = new MemoryStream();
            SongFile.Save(song, bpm, stream);
            stream.Position = 0;
            return SongFile.Load(stream);
        }

        [Test]
        public void ASong_SurvivesSavingAndLoading()
        {
            Song song = Song.CreateDefault();
            Track synth = song.Tracks[0];
            Track drums = song.Tracks[1];
            var melody = (NoteClip) synth.ClipAt(0);
            melody.SetTones(new[] { ToneAt(Note.CSharp, 5, 0, 48, 90), ToneAt(Note.G, 4, 96, 24, 127) });
            song.Place(synth, 2, melody);
            synth.Synth.Cutoff = 900;
            synth.Synth.FilterType = FilterType.BandPass;
            synth.Synth.LfoTarget = LfoTarget.Pitch;
            synth.Synth.Oscillator2Semitones = -7;
            var beat = (DrumClip) drums.ClipAt(0);
            beat.Pattern.SetVelocity(DrumSound.Kick, 0, DrumPattern.AccentVelocity);
            beat.Pattern.SetVelocity(DrumSound.ClosedHat, 2, DrumPattern.NormalVelocity);
            drums.Drums.Volume = 0.55;
            drums.IsMuted = true;
            Track second = song.AddTrack(TrackKind.Synth);
            song.Place(second, 1, song.CreateClip(second));

            LoadedSong loaded = RoundTrip(song, 137);

            Assert.That(loaded.BeatsPerMinute, Is.EqualTo(137));
            Assert.That(loaded.Song.Tracks.Select(t => t.Name), Is.EqualTo(new[] { "Synth", "Drums", "Synth 2" }));
            Track loadedSynth = loaded.Song.Tracks[0];
            var loadedMelody = (NoteClip) loadedSynth.ClipAt(0);
            Assert.That(loadedMelody.Tones.Select(t => (t.Pitch.ToString(), t.Velocity.Value, t.StartPosition.TotalTicks(TimeSignature.FourFour, 96))),
                Is.EqualTo(new[] { ("C#5", 90, 0), ("G4", 127, 96) }));
            Assert.That(loadedSynth.ClipAt(2), Is.SameAs(loadedMelody), "A clip in two sections is still one clip.");
            Assert.That(loadedSynth.ClipAt(1), Is.Null);
            Assert.That(loadedSynth.Synth.Cutoff, Is.EqualTo(900));
            Assert.That(loadedSynth.Synth.FilterType, Is.EqualTo(FilterType.BandPass));
            Assert.That(loadedSynth.Synth.LfoTarget, Is.EqualTo(LfoTarget.Pitch));
            Assert.That(loadedSynth.Synth.Oscillator2Semitones, Is.EqualTo(-7));

            Track loadedDrums = loaded.Song.Tracks[1];
            var loadedBeat = (DrumClip) loadedDrums.ClipAt(0);
            Assert.That(loadedBeat.Pattern.VelocityAt(DrumSound.Kick, 0), Is.EqualTo(DrumPattern.AccentVelocity));
            Assert.That(loadedBeat.Pattern.VelocityAt(DrumSound.ClosedHat, 2), Is.EqualTo(DrumPattern.NormalVelocity));
            Assert.That(loadedBeat.Pattern.HitCount, Is.EqualTo(2));
            Assert.That(loadedDrums.Drums.Volume, Is.EqualTo(0.55));
            Assert.That(loadedDrums.IsMuted, Is.True);
            Assert.That(loaded.Song.Tracks[2].ClipAt(1), Is.Not.Null);
        }

        [Test]
        public void TheFile_IsReadableJson()
        {
            var stream = new MemoryStream();
            SongFile.Save(Song.CreateDefault(), 120, stream);
            string json = Encoding.UTF8.GetString(stream.ToArray());

            Assert.That(json, Does.Contain("\"format\": \"sequencer-song\""));
            Assert.That(json, Does.Contain("\"kind\": \"Drums\""));
        }

        [TestCase("{}")]
        [TestCase("{\"format\": \"something-else\"}")]
        [TestCase("not json at all")]
        public void OtherFiles_AreRejectedClearly(string contents)
        {
            var error = Assert.Throws<FormatException>(() => SongFile.Load(new MemoryStream(Encoding.UTF8.GetBytes(contents))));

            Assert.That(error.Message, Does.Contain("song"));
        }

        [Test]
        public void SongsFromANewerVersion_AreRejected()
        {
            const string newer = "{\"format\": \"sequencer-song\", \"version\": 99, \"tracks\": []}";

            var error = Assert.Throws<FormatException>(() => SongFile.Load(new MemoryStream(Encoding.UTF8.GetBytes(newer))));

            Assert.That(error.Message, Does.Contain("newer version"));
        }

        [Test]
        public void ReplaceWith_TakesOverTheOtherSongsTracks()
        {
            Song song = Song.CreateDefault();
            var other = new Song();
            other.AddTrack(TrackKind.Drums);
            var changes = 0;
            song.Changed += (sender, args) => changes++;

            song.ReplaceWith(other);

            Assert.That(song.Tracks.Select(t => t.Kind), Is.EqualTo(new[] { TrackKind.Drums }));
            Assert.That(changes, Is.EqualTo(1));
        }
    }
}
