using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Sequencer.Domain;
using Sequencer.Domain.Arrangement;
using Sequencer.Domain.Drums;
using Sequencer.Domain.Settings;

namespace Sequencer.Midi.Tests
{
    [TestFixture]
    public class MidiFileWriterTests
    {
        private static readonly MidiProtocol Protocol = new MidiProtocol(new PitchAndPositionCalculator(new SequencerSettings()));

        private sealed class MidiTrack
        {
            public string Name;
            public int? Tempo;
            public readonly List<(int Tick, int Status, int Note, int Velocity)> Events = new List<(int, int, int, int)>();
        }

        /// <summary>
        /// A minimal Standard MIDI File reader, enough to check what the writer wrote.
        /// </summary>
        private static (int Format, int Division, List<MidiTrack> Tracks) Read(byte[] bytes)
        {
            var reader = new BinaryReader(new MemoryStream(bytes));
            int BigEndian(int count) => Enumerable.Range(0, count).Aggregate(0, (value, i) => (value << 8) | reader.ReadByte());
            int VariableLength()
            {
                int value = 0;
                byte b;
                do
                {
                    b = reader.ReadByte();
                    value = (value << 7) | (b & 0x7F);
                } while ((b & 0x80) != 0);

                return value;
            }

            Assert.That(Encoding.ASCII.GetString(reader.ReadBytes(4)), Is.EqualTo("MThd"));
            BigEndian(4);
            int format = BigEndian(2);
            int trackCount = BigEndian(2);
            int division = BigEndian(2);
            var tracks = new List<MidiTrack>();

            for (var t = 0; t < trackCount; t++)
            {
                Assert.That(Encoding.ASCII.GetString(reader.ReadBytes(4)), Is.EqualTo("MTrk"));
                long end = BigEndian(4) + reader.BaseStream.Position;
                var track = new MidiTrack();
                var time = 0;

                while (reader.BaseStream.Position < end)
                {
                    time += VariableLength();
                    byte status = reader.ReadByte();

                    if (status == 0xFF)
                    {
                        byte type = reader.ReadByte();
                        byte[] data = reader.ReadBytes(VariableLength());

                        if (type == 0x03)
                        {
                            track.Name = Encoding.ASCII.GetString(data);
                        }
                        else if (type == 0x51)
                        {
                            track.Tempo = (data[0] << 16) | (data[1] << 8) | data[2];
                        }
                    }
                    else
                    {
                        track.Events.Add((time, status, reader.ReadByte(), reader.ReadByte()));
                    }
                }

                tracks.Add(track);
            }

            return (format, division, tracks);
        }

        private static byte[] Write(Song song, int bpm)
        {
            var stream = new MemoryStream();
            new MidiFileWriter(Protocol).Write(song, bpm, stream);
            return stream.ToArray();
        }

        private static Song ASong()
        {
            Song song = Song.CreateDefault();
            var melody = (NoteClip) song.Tracks[0].ClipAt(0);
            melody.SetTones(new[]
            {
                new Tone(new Pitch(Note.C, 5), new Velocity(100), Position.PositionFromTick(0, TimeSignature.FourFour, 96), Position.PositionFromTick(96, TimeSignature.FourFour, 96))
            });
            song.Place(song.Tracks[0], 1, melody);
            ((DrumClip) song.Tracks[1].ClipAt(0)).Pattern.SetVelocity(DrumSound.Snare, 4, 96);
            return song;
        }

        [Test]
        public void TheFile_IsType1_WithATempoTrackAndOneTrackPerInstrument()
        {
            var (format, division, tracks) = Read(Write(ASong(), 125));

            Assert.That(format, Is.EqualTo(1));
            Assert.That(division, Is.EqualTo(96));
            Assert.That(tracks.Count, Is.EqualTo(3));
            Assert.That(tracks[0].Tempo, Is.EqualTo(60000000 / 125));
            Assert.That(tracks.Skip(1).Select(t => t.Name), Is.EqualTo(new[] { "Synth", "Drums" }));
        }

        [Test]
        public void Notes_ArePlacedInEverySectionTheirClipPlays()
        {
            var (_, _, tracks) = Read(Write(ASong(), 120));
            int middleC = Protocol.ProtocolNoteNumber(new Pitch(Note.C, 5));

            List<(int Tick, int Status, int Note, int Velocity)> noteOns = tracks[1].Events.Where(e => (e.Status & 0xF0) == 0x90).ToList();

            Assert.That(noteOns.Select(e => (e.Tick, e.Note, e.Velocity)), Is.EqualTo(new[] { (0, middleC, 100), (1536, middleC, 100) }));
            Assert.That(tracks[1].Events.Where(e => (e.Status & 0xF0) == 0x80).Select(e => e.Tick), Is.EqualTo(new[] { 96, 1632 }));
        }

        [Test]
        public void Drums_UseChannel10AndGeneralMidiNotes()
        {
            var (_, _, tracks) = Read(Write(ASong(), 120));

            (int Tick, int Status, int Note, int Velocity) snare = tracks[2].Events.Single(e => (e.Status & 0xF0) == 0x90);

            Assert.That(snare.Status & 0x0F, Is.EqualTo(9), "Channel 10.");
            Assert.That(snare.Note, Is.EqualTo(38), "Acoustic snare.");
            Assert.That(snare.Tick, Is.EqualTo(4 * 24));
        }

        [Test]
        public void MutedTracks_AreLeftOut()
        {
            Song song = ASong();
            song.Tracks[1].IsMuted = true;

            var (_, _, tracks) = Read(Write(song, 120));

            Assert.That(tracks.Skip(1).Select(t => t.Name), Is.EqualTo(new[] { "Synth" }));
        }

        [TestCase(0, new byte[] { 0x00 })]
        [TestCase(127, new byte[] { 0x7F })]
        [TestCase(128, new byte[] { 0x81, 0x00 })]
        [TestCase(16383, new byte[] { 0xFF, 0x7F })]
        [TestCase(16384, new byte[] { 0x81, 0x80, 0x00 })]
        public void VariableLengthNumbers_AreEncodedAsMidiExpects(int value, byte[] expected)
        {
            Assert.That(MidiFileWriter.VariableLength(value), Is.EqualTo(expected));
        }
    }
}
