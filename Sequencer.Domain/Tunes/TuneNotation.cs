using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using JetBrains.Annotations;

namespace Sequencer.Domain.Tunes
{
    /// <summary>
    /// A note in a tune, placed in sixteenth-note steps.
    /// </summary>
    public readonly struct TuneNote
    {
        public TuneNote([NotNull] Pitch pitch, int start, int length)
        {
            Pitch = pitch;
            Start = start;
            Length = length;
        }

        [NotNull]
        public Pitch Pitch { get; }

        /// <summary>
        /// The first sixteenth it plays on, counting from 0.
        /// </summary>
        public int Start { get; }

        /// <summary>
        /// In sixteenth notes.
        /// </summary>
        public int Length { get; }
    }

    /// <summary>
    /// Reads one voice of a tune written as notes and rests with lengths in sixteenths, for example
    /// "E4:4 E4:4 F4:4 G4:4 | G4:4 F4:4 E4:4 D4:4". Pitches use standard names (middle C is C4), with sharps
    /// or flats. "r" is a rest. "|" marks a bar line, and every bar must add up to 16 sixteenths.
    /// </summary>
    public static class TuneNotation
    {
        public const int SixteenthsPerBar = 16;

        // This app numbers octaves one higher than standard pitch notation (its C5 is middle C).
        private const int AppOctaveOffset = 1;

        [NotNull] private static readonly Regex Token = new Regex(@"^(?:(?<rest>r)|(?<note>[A-G])(?<accidental>#|b)?(?<octave>-?\d)):(?<length>\d+)$");

        [NotNull] private static readonly string[] SharpNames = { "C", "C#", "D", "D#", "E", "F", "F#", "G", "G#", "A", "A#", "B" };

        [NotNull]
        public static IReadOnlyList<TuneNote> Parse([NotNull] string notation)
        {
            var notes = new List<TuneNote>();
            var position = 0;
            var barStart = 0;
            var bar = 1;

            foreach (string token in notation.Split((char[]) null, StringSplitOptions.RemoveEmptyEntries))
            {
                if (token == "|")
                {
                    CheckBarLength(position - barStart, bar);
                    barStart = position;
                    bar++;
                    continue;
                }

                Match match = Token.Match(token);

                if (!match.Success)
                {
                    throw new FormatException($"Can't read \"{token}\". Write notes like \"C#4:2\" and rests like \"r:4\".");
                }

                int length = int.Parse(match.Groups["length"].Value);

                if (length <= 0)
                {
                    throw new FormatException($"\"{token}\" has no length.");
                }

                if (!match.Groups["rest"].Success)
                {
                    notes.Add(new TuneNote(ParsePitch(match), position, length));
                }

                position += length;
            }

            if (barStart > 0 || notation.Contains("|"))
            {
                CheckBarLength(position - barStart, bar);
            }

            return notes;
        }

        [NotNull]
        private static Pitch ParsePitch([NotNull] Match match)
        {
            int semitone = Array.IndexOf(SharpNames, match.Groups["note"].Value);
            int octave = int.Parse(match.Groups["octave"].Value);

            switch (match.Groups["accidental"].Value)
            {
                case "#":
                    semitone++;
                    break;
                case "b":
                    semitone--;
                    break;
            }

            // B# is the next octave's C, and Cb the previous octave's B.
            if (semitone >= 12)
            {
                semitone -= 12;
                octave++;
            }
            else if (semitone < 0)
            {
                semitone += 12;
                octave--;
            }

            return new Pitch(Note.FromDisplayName(SharpNames[semitone]), octave + AppOctaveOffset);
        }

        private static void CheckBarLength(int length, int bar)
        {
            if (length != SixteenthsPerBar)
            {
                throw new FormatException($"Bar {bar} is {length} sixteenths long; a bar of 4/4 is {SixteenthsPerBar}.");
            }
        }
    }
}
