using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;

namespace Sequencer.Domain.Drums
{
    /// <summary>
    /// A ready-made drum pattern. Each sound's steps are written as a grid: "x" plays, "X" plays accented,
    /// "." is silent, and spaces are ignored (use them to group beats). A one-bar line (16 steps) repeats to
    /// fill the four bars; a four-bar line (64 steps) is used as written.
    /// </summary>
    public sealed class DrumLoop
    {
        public DrumLoop([NotNull] string name, [NotNull] string style, int beatsPerMinute, [NotNull] IReadOnlyDictionary<DrumSound, string> grid)
        {
            Name = name;
            Style = style;
            BeatsPerMinute = beatsPerMinute;
            Grid = grid;
        }

        [NotNull]
        public string Name { get; }

        /// <summary>
        /// A short description, such as "House, disco".
        /// </summary>
        [NotNull]
        public string Style { get; }

        public int BeatsPerMinute { get; }

        [NotNull]
        public IReadOnlyDictionary<DrumSound, string> Grid { get; }

        [NotNull]
        public DrumPattern ToPattern()
        {
            var pattern = new DrumPattern();

            foreach (KeyValuePair<DrumSound, string> lane in Grid)
            {
                string cells = new string(lane.Value.Where(c => !char.IsWhiteSpace(c)).ToArray());

                if (cells.Length != 16 && cells.Length != DrumPattern.StepCount)
                {
                    throw new FormatException($"{Name}: the {lane.Key} line has {cells.Length} steps; write 16 (one bar) or 64 (four bars).");
                }

                for (var step = 0; step < DrumPattern.StepCount; step++)
                {
                    char cell = cells[step % cells.Length];

                    switch (cell)
                    {
                        case 'x':
                            pattern.SetVelocity(lane.Key, step, DrumPattern.NormalVelocity);
                            break;
                        case 'X':
                            pattern.SetVelocity(lane.Key, step, DrumPattern.AccentVelocity);
                            break;
                        case '.':
                            break;
                        default:
                            throw new FormatException($"{Name}: \"{cell}\" in the {lane.Key} line; use x, X or \".\".");
                    }
                }
            }

            return pattern;
        }

        public override string ToString() => Name;
    }
}
