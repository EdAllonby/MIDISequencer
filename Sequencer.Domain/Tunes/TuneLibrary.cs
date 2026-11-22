using System.Collections.Generic;
using JetBrains.Annotations;
using Sequencer.Domain.Settings;

namespace Sequencer.Domain.Tunes
{
    /// <summary>
    /// Well-known tunes to start from. All are traditional or public-domain classical pieces, cut to their
    /// most recognisable four bars. Notation: standard pitch names (middle C is C4), lengths in sixteenths,
    /// "r" for a rest, "|" between bars.
    /// </summary>
    public static class TuneLibrary
    {
        private const int Melody = 100;
        private const int Harmony = 70;
        private const int Bass = 76;

        [NotNull]
        [ItemNotNull]
        public static IReadOnlyList<Tune> All { get; } = new[]
        {
            new Tune("Ode to Joy", "Beethoven, 1824", 120, Waveform.Triangle,
                new TuneVoice("E4:4 E4:4 F4:4 G4:4 | G4:4 F4:4 E4:4 D4:4 | C4:4 C4:4 D4:4 E4:4 | E4:6 D4:2 D4:8", Melody),
                new TuneVoice("C3:16 | G2:16 | C3:16 | G2:16", Bass)),

            new Tune("Twinkle, Twinkle, Little Star", "Traditional, 1761", 100, Waveform.Sine,
                new TuneVoice("C4:4 C4:4 G4:4 G4:4 | A4:4 A4:4 G4:8 | F4:4 F4:4 E4:4 E4:4 | D4:4 D4:4 C4:8", Melody),
                new TuneVoice("C3:16 | F2:8 C3:8 | F2:8 C3:8 | G2:8 C3:8", Bass)),

            // Sung as a round: the second voice comes in two bars later.
            new Tune("Frère Jacques", "Traditional, 18th century", 112, Waveform.Triangle,
                new TuneVoice("C4:4 D4:4 E4:4 C4:4 | C4:4 D4:4 E4:4 C4:4 | E4:4 F4:4 G4:8 | E4:4 F4:4 G4:8", Melody),
                new TuneVoice("r:16 | r:16 | C3:4 D3:4 E3:4 C3:4 | C3:4 D3:4 E3:4 C3:4", Harmony)),

            new Tune("Jingle Bells", "James Lord Pierpont, 1857", 126, Waveform.Square,
                new TuneVoice("E4:4 E4:4 E4:8 | E4:4 E4:4 E4:8 | E4:4 G4:4 C4:6 D4:2 | E4:16", Melody),
                new TuneVoice("C3:8 G2:8 | C3:8 G2:8 | C3:8 G2:8 | C3:16", Bass)),

            // Written in 3/8, so there are no 4/4 bar lines; each step is one of the score's sixteenths.
            new Tune("Für Elise", "Beethoven, 1810", 120, Waveform.Triangle,
                new TuneVoice(
                    "E5:1 D#5:1 E5:1 D#5:1 E5:1 B4:1 D5:1 C5:1 A4:2 r:1 C4:1 E4:1 A4:1 B4:2 r:1 E4:1 G#4:1 B4:1 " +
                    "C5:2 r:1 E4:1 E5:1 D#5:1 E5:1 D#5:1 E5:1 B4:1 D5:1 C5:1 A4:2 r:1 C4:1 E4:1 A4:1 B4:2 r:1 E4:1 C5:1 B4:1 A4:4",
                    Melody),
                new TuneVoice(
                    "r:8 A2:1 E3:1 A3:1 r:3 E2:1 E3:1 G#3:1 r:3 A2:1 E3:1 A3:1 r:9 A2:1 E3:1 A3:1 r:3 E2:1 E3:1 G#3:1 r:3 A2:1 E3:1 A3:1",
                    Harmony)),

            // The Russian folk song best known as the Tetris theme.
            new Tune("Korobeiniki", "Russian folk song, 19th century", 140, Waveform.Square,
                new TuneVoice("E5:4 B4:2 C5:2 D5:4 C5:2 B4:2 | A4:4 A4:2 C5:2 E5:4 D5:2 C5:2 | B4:6 C5:2 D5:4 E5:4 | C5:4 A4:4 A4:8", Melody),
                new TuneVoice(
                    "E2:2 E3:2 E2:2 E3:2 E2:2 E3:2 E2:2 E3:2 | A2:2 A3:2 A2:2 A3:2 A2:2 A3:2 A2:2 A3:2 | " +
                    "G#2:2 G#3:2 G#2:2 G#3:2 E2:2 E3:2 E2:2 E3:2 | A2:2 A3:2 A2:2 A3:2 A2:2 A3:2 A2:2 A3:2",
                    Bass)),

            new Tune("In the Hall of the Mountain King", "Grieg, 1875", 120, Waveform.Sawtooth,
                new TuneVoice("A3:2 B3:2 C4:2 D4:2 E4:2 C4:2 E4:4 | D#4:2 B3:2 D#4:4 D4:2 Bb3:2 D4:4 | A3:2 B3:2 C4:2 D4:2 E4:2 C4:2 E4:2 A4:2 | G4:2 E4:2 C4:2 E4:2 G4:8", Melody),
                new TuneVoice("A2:2 r:2 A2:2 r:2 A2:2 r:2 A2:2 r:2 | B2:2 r:2 B2:2 r:2 Bb2:2 r:2 Bb2:2 r:2 | A2:2 r:2 A2:2 r:2 A2:2 r:2 A2:2 r:2 | E2:2 r:2 E2:2 r:2 E2:2 r:2 E2:2 r:2", Bass)),

            // The famous descending line over the ground bass.
            new Tune("Canon in D", "Pachelbel, c. 1680", 84, Waveform.Sine,
                new TuneVoice("F#5:8 E5:8 | D5:8 C#5:8 | B4:8 A4:8 | B4:8 C#5:8", Melody),
                new TuneVoice("A4:8 A4:8 | F#4:8 A4:8 | D4:8 F#4:8 | G4:8 E4:8", Harmony),
                new TuneVoice("D3:8 A2:8 | B2:8 F#2:8 | G2:8 D2:8 | G2:8 A2:8", Bass))
        };
    }
}
