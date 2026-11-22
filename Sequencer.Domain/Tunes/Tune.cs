using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Sequencer.Domain.Settings;

namespace Sequencer.Domain.Tunes
{
    /// <summary>
    /// One part of a tune (a melody, a bassline) written in <see cref="TuneNotation"/>.
    /// </summary>
    public sealed class TuneVoice
    {
        public TuneVoice([NotNull] string notation, int velocity)
        {
            Notation = notation;
            Velocity = velocity;
        }

        [NotNull]
        public string Notation { get; }

        public int Velocity { get; }
    }

    /// <summary>
    /// A ready-made tune to load into the sequencer, with a tempo and sound that suit it.
    /// </summary>
    public sealed class Tune
    {
        public Tune([NotNull] string name, [NotNull] string credit, int beatsPerMinute, Waveform waveform, [NotNull] params TuneVoice[] voices)
        {
            Name = name;
            Credit = credit;
            BeatsPerMinute = beatsPerMinute;
            Waveform = waveform;
            Voices = voices;
        }

        [NotNull]
        public string Name { get; }

        /// <summary>
        /// Who wrote it and when, such as "Beethoven, 1824".
        /// </summary>
        [NotNull]
        public string Credit { get; }

        public int BeatsPerMinute { get; }

        public Waveform Waveform { get; }

        [NotNull]
        [ItemNotNull]
        public IReadOnlyList<TuneVoice> Voices { get; }

        /// <summary>
        /// Every note in every voice, as tones on the sequencer's grid.
        /// </summary>
        [NotNull]
        [ItemNotNull]
        public IReadOnlyList<Tone> ToTones([NotNull] TimeSignature timeSignature, int ticksPerQuarterNote)
        {
            int ticksPerSixteenth = ticksPerQuarterNote / 4;

            return Voices.SelectMany(voice => TuneNotation.Parse(voice.Notation).Select(note => new Tone(
                    note.Pitch,
                    new Velocity(voice.Velocity),
                    Position.PositionFromTick(note.Start * ticksPerSixteenth, timeSignature, ticksPerQuarterNote),
                    Position.PositionFromTick((note.Start + note.Length) * ticksPerSixteenth, timeSignature, ticksPerQuarterNote))))
                .ToList();
        }

        public override string ToString() => Name;
    }
}
