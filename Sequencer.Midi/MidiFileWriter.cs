using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using JetBrains.Annotations;
using Sequencer.Domain;
using Sequencer.Domain.Arrangement;
using Sequencer.Domain.Drums;

namespace Sequencer.Midi
{
    /// <summary>
    /// Writes the song as a Standard MIDI File (type 1), for opening in a DAW such as GarageBand or Logic:
    /// a tempo track, one track per synth track, and drum tracks on channel 10 using General MIDI drum notes.
    /// Muted tracks are left out, so the file has what you hear.
    /// </summary>
    public sealed class MidiFileWriter
    {
        public const int TicksPerQuarterNote = 96;
        private const int TicksPerSection = TicksPerQuarterNote * 16;
        private const int DrumChannel = 9;

        /// <summary>
        /// General MIDI percussion notes for each drum sound.
        /// </summary>
        [NotNull] public static readonly IReadOnlyDictionary<DrumSound, int> GeneralMidiDrums = new Dictionary<DrumSound, int>
        {
            { DrumSound.Kick, 36 },
            { DrumSound.Snare, 38 },
            { DrumSound.Clap, 39 },
            { DrumSound.ClosedHat, 42 },
            { DrumSound.Tom, 45 },
            { DrumSound.OpenHat, 46 }
        };

        [NotNull] private readonly IDigitalAudioProtocol protocol;

        public MidiFileWriter([NotNull] IDigitalAudioProtocol protocol)
        {
            this.protocol = protocol;
        }

        public void Write([NotNull] Song song, int beatsPerMinute, [NotNull] Stream stream)
        {
            List<Track> tracks = song.Tracks.Where(track => !track.IsMuted).ToList();
            var chunks = new List<byte[]> { TempoTrack(beatsPerMinute) };
            var channel = 0;

            foreach (Track track in tracks)
            {
                if (track.Kind == TrackKind.Drums)
                {
                    chunks.Add(TrackChunk(track.Name, DrumEvents(song, track)));
                }
                else
                {
                    chunks.Add(TrackChunk(track.Name, NoteEvents(song, track, channel)));
                    channel = channel + 1 == DrumChannel ? channel + 2 : channel + 1;
                }
            }

            var writer = new BinaryWriter(stream);
            writer.Write(Encoding.ASCII.GetBytes("MThd"));
            WriteBigEndian(writer, 6, 4);
            WriteBigEndian(writer, 1, 2);
            WriteBigEndian(writer, chunks.Count, 2);
            WriteBigEndian(writer, TicksPerQuarterNote, 2);

            foreach (byte[] chunk in chunks)
            {
                writer.Write(chunk);
            }

            writer.Flush();
        }

        private IEnumerable<MidiEvent> NoteEvents([NotNull] Song song, [NotNull] Track track, int channel)
        {
            for (var section = 0; section < song.LengthInSections; section++)
            {
                if (!(track.ClipAt(section) is NoteClip clip))
                {
                    continue;
                }

                foreach (Tone tone in clip.Tones)
                {
                    int note = protocol.ProtocolNoteNumber(tone.Pitch);
                    int start = section * TicksPerSection + tone.StartPosition.TotalTicks(TimeSignature.FourFour, TicksPerQuarterNote);
                    int end = section * TicksPerSection + tone.EndPosition.TotalTicks(TimeSignature.FourFour, TicksPerQuarterNote);
                    yield return new MidiEvent(start, (byte) (0x90 | channel), note, Math.Max(1, tone.Velocity.Value));
                    yield return new MidiEvent(end, (byte) (0x80 | channel), note, 0);
                }
            }
        }

        private static IEnumerable<MidiEvent> DrumEvents([NotNull] Song song, [NotNull] Track track)
        {
            const int ticksPerStep = TicksPerQuarterNote / 4;

            for (var section = 0; section < song.LengthInSections; section++)
            {
                if (!(track.ClipAt(section) is DrumClip clip))
                {
                    continue;
                }

                foreach (DrumSound sound in DrumPattern.AllSounds)
                {
                    for (var step = 0; step < DrumPattern.StepCount; step++)
                    {
                        byte velocity = clip.Pattern.VelocityAt(sound, step);

                        if (velocity > 0)
                        {
                            int start = section * TicksPerSection + step * ticksPerStep;
                            yield return new MidiEvent(start, 0x90 | DrumChannel, GeneralMidiDrums[sound], velocity);
                            yield return new MidiEvent(start + ticksPerStep / 2, 0x80 | DrumChannel, GeneralMidiDrums[sound], 0);
                        }
                    }
                }
            }
        }

        [NotNull]
        private static byte[] TempoTrack(int beatsPerMinute)
        {
            int microsecondsPerQuarter = 60000000 / beatsPerMinute;
            var data = new List<byte>();
            data.AddRange(new byte[] { 0x00, 0xFF, 0x51, 0x03, (byte) (microsecondsPerQuarter >> 16), (byte) (microsecondsPerQuarter >> 8), (byte) microsecondsPerQuarter });
            data.AddRange(new byte[] { 0x00, 0xFF, 0x58, 0x04, 0x04, 0x02, 0x18, 0x08 });
            data.AddRange(new byte[] { 0x00, 0xFF, 0x2F, 0x00 });
            return Chunk(data);
        }

        [NotNull]
        private static byte[] TrackChunk([NotNull] string name, [NotNull] IEnumerable<MidiEvent> events)
        {
            var data = new List<byte> { 0x00, 0xFF, 0x03 };
            byte[] nameBytes = Encoding.ASCII.GetBytes(name);
            data.AddRange(VariableLength(nameBytes.Length));
            data.AddRange(nameBytes);

            var time = 0;

            // Note-offs before note-ons at the same tick, so a repeated note isn't cut off by its own end.
            foreach (MidiEvent midiEvent in events.OrderBy(e => e.Tick).ThenBy(e => e.Status & 0xF0))
            {
                data.AddRange(VariableLength(midiEvent.Tick - time));
                data.Add(midiEvent.Status);
                data.Add((byte) midiEvent.Note);
                data.Add((byte) midiEvent.Velocity);
                time = midiEvent.Tick;
            }

            data.AddRange(new byte[] { 0x00, 0xFF, 0x2F, 0x00 });
            return Chunk(data);
        }

        [NotNull]
        private static byte[] Chunk([NotNull] List<byte> data)
        {
            var chunk = new List<byte>(Encoding.ASCII.GetBytes("MTrk"));
            chunk.AddRange(new[] { (byte) (data.Count >> 24), (byte) (data.Count >> 16), (byte) (data.Count >> 8), (byte) data.Count });
            chunk.AddRange(data);
            return chunk.ToArray();
        }

        /// <summary>
        /// MIDI's variable-length quantity: 7 bits per byte, high bit set on all but the last.
        /// </summary>
        [NotNull]
        public static byte[] VariableLength(int value)
        {
            var bytes = new Stack<byte>();
            bytes.Push((byte) (value & 0x7F));

            while ((value >>= 7) > 0)
            {
                bytes.Push((byte) ((value & 0x7F) | 0x80));
            }

            return bytes.ToArray();
        }

        private static void WriteBigEndian([NotNull] BinaryWriter writer, int value, int bytes)
        {
            for (int shift = (bytes - 1) * 8; shift >= 0; shift -= 8)
            {
                writer.Write((byte) (value >> shift));
            }
        }

        private readonly struct MidiEvent
        {
            public MidiEvent(int tick, int status, int note, int velocity)
            {
                Tick = tick;
                Status = (byte) status;
                Note = note;
                Velocity = velocity;
            }

            public int Tick { get; }

            public byte Status { get; }

            public int Note { get; }

            public int Velocity { get; }
        }
    }
}
