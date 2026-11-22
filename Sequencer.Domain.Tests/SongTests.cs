using System;
using System.Collections.Generic;
using System.Linq;
using Moq;
using NUnit.Framework;
using Sequencer.Domain.Arrangement;
using Sequencer.Domain.Drums;

namespace Sequencer.Domain.Tests
{
    [TestFixture]
    public class SongTests
    {
        private static Tone ToneAt(int sixteenth) =>
            new Tone(new Pitch(Note.C, 5), new Velocity(100), Position.PositionFromTick(sixteenth * 24, TimeSignature.FourFour, 96),
                Position.PositionFromTick((sixteenth + 1) * 24, TimeSignature.FourFour, 96));

        [Test]
        public void TheDefaultSong_HasASynthAndADrumTrack_EachWithAClipInTheFirstSection()
        {
            Song song = Song.CreateDefault();

            Assert.That(song.Tracks.Select(t => t.Name), Is.EqualTo(new[] { "Synth", "Drums" }));
            Assert.That(song.Tracks[0].ClipAt(0), Is.InstanceOf<NoteClip>());
            Assert.That(song.Tracks[1].ClipAt(0), Is.InstanceOf<DrumClip>());
            Assert.That(song.Tracks[0].Synth, Is.Not.Null);
            Assert.That(song.Tracks[1].Drums, Is.Not.Null);
        }

        [Test]
        public void Clips_AreNamedWithTheTracksNextFreeLetter()
        {
            var song = new Song();
            Track track = song.AddTrack(TrackKind.Synth);

            IEnumerable<string> names = Enumerable.Range(0, 28).Select(i => song.CreateClip(track).Name).ToList();

            Assert.That(names.Take(3), Is.EqualTo(new[] { "A", "B", "C" }));
            Assert.That(names.Skip(26), Is.EqualTo(new[] { "AA", "AB" }));
        }

        [Test]
        public void TracksOfTheSameKind_AreNumbered()
        {
            var song = new Song();

            song.AddTrack(TrackKind.Drums);
            Track second = song.AddTrack(TrackKind.Drums);

            Assert.That(second.Name, Is.EqualTo("Drums 2"));
            Assert.That(second.ColourIndex, Is.EqualTo(1));
        }

        [Test]
        public void AClip_CanOnlyBePlacedOnItsOwnTrack()
        {
            Song song = Song.CreateDefault();
            Clip synthClip = song.Tracks[0].ClipAt(0);

            Assert.Throws<ArgumentException>(() => song.Place(song.Tracks[1], 1, synthClip));
        }

        [Test]
        public void LengthInSections_RunsToTheLastClip()
        {
            Song song = Song.CreateDefault();
            Assert.That(song.LengthInSections, Is.EqualTo(1));

            song.Place(song.Tracks[1], 5, song.Tracks[1].ClipAt(0));

            Assert.That(song.LengthInSections, Is.EqualTo(6));
        }

        [Test]
        public void Placing_RaisesChanged()
        {
            Song song = Song.CreateDefault();
            var changes = 0;
            song.Changed += (sender, args) => changes++;

            song.Place(song.Tracks[0], 1, song.Tracks[0].ClipAt(0));

            Assert.That(changes, Is.EqualTo(1));
        }

        [Test]
        public void CopyClip_MakesAnIndependentClip()
        {
            Song song = Song.CreateDefault();
            var drums = (DrumClip) song.Tracks[1].ClipAt(0);
            drums.Pattern.SetVelocity(DrumSound.Kick, 0, 96);

            var copy = (DrumClip) song.CopyClip(drums);
            drums.Pattern.Clear();

            Assert.That(copy.Name, Is.EqualTo("B"));
            Assert.That(copy.Pattern.IsHit(DrumSound.Kick, 0), Is.True);
        }

        [Test]
        public void ANoteClip_PlaysFromTheEditorWhileOpen_AndKeepsACopyWhenClosed()
        {
            Song song = Song.CreateDefault();
            var clip = (NoteClip) song.Tracks[0].ClipAt(0);
            clip.SetTones(new[] { ToneAt(0) });

            var editorNote = Mock.Of<ISequencedNote>(n => n.Tone == ToneAt(4));
            var editor = new Mock<ISequencedNotes>();
            editor.Setup(e => e.AllNotes).Returns(new[] { editorNote });

            clip.Open(editor.Object);
            Assert.That(clip.IsOpen, Is.True);
            Assert.That(clip.AllNotes, Is.EqualTo(new[] { editorNote }), "Reads the editor's notes while open.");

            clip.Close();
            Assert.That(clip.IsOpen, Is.False);
            Assert.That(clip.Tones.Single().StartPosition, Is.EqualTo(ToneAt(4).StartPosition), "Keeps the editor's notes.");
        }

        [Test]
        public void AStoredNoteClip_FindsNotesByStartAndEnd()
        {
            Song song = Song.CreateDefault();
            var clip = (NoteClip) song.Tracks[0].ClipAt(0);
            Tone tone = ToneAt(3);
            clip.SetTones(new[] { tone });

            Assert.That(clip.FindNotesFromStartingPosition(tone.StartPosition).Single().Tone, Is.SameAs(tone));
            Assert.That(clip.FindNotesFromEndingPosition(tone.EndPosition).Single().Tone, Is.SameAs(tone));
            Assert.That(clip.FindNotesFromStartingPosition(tone.EndPosition), Is.Empty);
        }

        [Test]
        public void ASong_HasAtMostEightTracks()
        {
            var song = new Song();

            for (var i = 0; i < Song.MaximumTracks; i++)
            {
                song.AddTrack(TrackKind.Synth);
            }

            Assert.Throws<InvalidOperationException>(() => song.AddTrack(TrackKind.Drums));
        }
    }
}
