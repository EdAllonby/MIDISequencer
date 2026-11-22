using System.Collections.Generic;
using JetBrains.Annotations;

namespace Sequencer.Domain.Drums
{
    /// <summary>
    /// Classic drum patterns to start from. Each line is one sound; see <see cref="DrumLoop"/> for the grid
    /// notation. Groups of four steps are one beat.
    /// </summary>
    public static class DrumLoopLibrary
    {
        [NotNull]
        [ItemNotNull]
        public static IReadOnlyList<DrumLoop> All { get; } = new[]
        {
            new DrumLoop("Four on the Floor", "House, disco", 124, new Dictionary<DrumSound, string>
            {
                [DrumSound.OpenHat] = "..x. ..x. ..x. ..x.",
                [DrumSound.ClosedHat] = "x... x... x... x...",
                [DrumSound.Clap] = ".... X... .... X...",
                [DrumSound.Kick] = "X... x... X... x..."
            }),

            new DrumLoop("Rock Beat", "Rock, pop", 112, new Dictionary<DrumSound, string>
            {
                [DrumSound.ClosedHat] = "X.x. x.x. X.x. x.x.",
                [DrumSound.Snare] = ".... X... .... X...",
                [DrumSound.Kick] = "X... ..x. X.x. ...."
            }),

            new DrumLoop("Boom Bap", "Hip-hop", 90, new Dictionary<DrumSound, string>
            {
                [DrumSound.ClosedHat] = "x.x. x.x. x.x. x.xx",
                [DrumSound.Snare] = ".... X... .... X...",
                [DrumSound.Kick] = "X... ...x ..X. ...."
            }),

            new DrumLoop("Breakbeat", "Funk, breaks", 100, new Dictionary<DrumSound, string>
            {
                [DrumSound.ClosedHat] = "x.x. x.x. x.x. x.x.",
                [DrumSound.Snare] = ".... X..x .x.. X...",
                [DrumSound.Kick] = "X.x. .... ..X. .x.."
            }),

            new DrumLoop("Disco", "Disco, funk", 118, new Dictionary<DrumSound, string>
            {
                [DrumSound.OpenHat] = "..X. ..X. ..X. ..X.",
                [DrumSound.ClosedHat] = "x.x. x.x. x.x. x.x.",
                [DrumSound.Snare] = ".... X... .... X...",
                [DrumSound.Kick] = "X... X... X... X..."
            }),

            new DrumLoop("Dembow", "Reggaeton", 96, new Dictionary<DrumSound, string>
            {
                [DrumSound.ClosedHat] = "x.x. x.x. x.x. x.x.",
                [DrumSound.Snare] = "...x ..x. ...x ..x.",
                [DrumSound.Kick] = "X... X... X... X..."
            }),

            new DrumLoop("Bossa Nova", "Brazilian", 132, new Dictionary<DrumSound, string>
            {
                [DrumSound.ClosedHat] = "x.x. x.x. x.x. x.x.",
                [DrumSound.Tom] = "x..x ..x. .x.. x... x..x ..x. ..x. .x..",
                [DrumSound.Kick] = "X..x X..x X..x X..x"
            }.Repeat(DrumSound.Tom)),

            new DrumLoop("Trap", "Trap, hip-hop", 140, new Dictionary<DrumSound, string>
            {
                [DrumSound.OpenHat] = ".... .... .... ...x",
                [DrumSound.ClosedHat] = "x.x. x.x. x.xx x.x. x.x. x.x. xxxx x.x.",
                [DrumSound.Clap] = ".... .... X... .... .... .... X... ....",
                [DrumSound.Kick] = "X... .... ..X. .... X.X. .... .... ..x."
            }.Repeat(DrumSound.ClosedHat, DrumSound.Clap, DrumSound.Kick))
        };

        /// <summary>
        /// Repeats two-bar lines to fill four bars.
        /// </summary>
        [NotNull]
        private static Dictionary<DrumSound, string> Repeat([NotNull] this Dictionary<DrumSound, string> grid, [NotNull] params DrumSound[] twoBarLines)
        {
            foreach (DrumSound sound in twoBarLines)
            {
                grid[sound] = grid[sound] + " " + grid[sound];
            }

            return grid;
        }
    }
}
