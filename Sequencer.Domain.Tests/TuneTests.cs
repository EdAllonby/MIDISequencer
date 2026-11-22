using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Sequencer.Domain.Settings;
using Sequencer.Domain.Tunes;
using Sequencer.Midi;

namespace Sequencer.Domain.Tests
{
    [TestFixture]
    public class TuneTests
    {
        private static readonly SequencerSettings Settings = new SequencerSettings();
        private static readonly MidiProtocol Protocol = new MidiProtocol(new PitchAndPositionCalculator(Settings));

        private static IEnumerable<Tune> Tunes => TuneLibrary.All;

        [Test]
        public void Parse_ReadsNotesRestsAndLengths()
        {
            IReadOnlyList<TuneNote> notes = TuneNotation.Parse("C4:4 r:2 E4:2 G4:8");

            Assert.That(notes.Select(n => (n.Pitch.ToString(), n.Start, n.Length)),
                Is.EqualTo(new[] { ("C5", 0, 4), ("E5", 6, 2), ("G5", 8, 8) }),
                "Standard C4 (middle C) is this app's C5.");
        }

        [TestCase("Bb3:1", "A#4")]
        [TestCase("Db4:1", "C#5")]
        [TestCase("Cb4:1", "B4")]
        [TestCase("B#3:1", "C5")]
        [TestCase("F#2:1", "F#3")]
        public void Parse_ReadsSharpsAndFlats(string token, string expectedPitch)
        {
            Assert.That(TuneNotation.Parse(token).Single().Pitch.ToString(), Is.EqualTo(expectedPitch));
        }

        [Test]
        public void Parse_RejectsABarOfTheWrongLength()
        {
            var error = Assert.Throws<FormatException>(() => TuneNotation.Parse("C4:4 C4:4 C4:4 C4:4 | C4:4 C4:4 C4:4"));

            Assert.That(error.Message, Does.Contain("Bar 2 is 12 sixteenths"));
        }

        [Test]
        public void Parse_RejectsUnreadableTokens()
        {
            Assert.Throws<FormatException>(() => TuneNotation.Parse("H4:4"));
            Assert.Throws<FormatException>(() => TuneNotation.Parse("C4"));
            Assert.Throws<FormatException>(() => TuneNotation.Parse("C4:0"));
        }

        [TestCaseSource(nameof(Tunes))]
        public void EveryTune_FitsTheSequencer(Tune tune)
        {
            int lowest = Protocol.ProtocolNoteNumber(Settings.LowestPitch);
            int highest = lowest + Settings.TotalNotes - 1;
            int sequenceLength = TuneNotation.SixteenthsPerBar * Settings.TimeSignature.BarsPerMeasure * Settings.TotalMeasures;

            foreach (TuneVoice voice in tune.Voices)
            {
                IReadOnlyList<TuneNote> notes = TuneNotation.Parse(voice.Notation);

                Assert.That(notes, Is.Not.Empty);
                Assert.That(notes.Max(n => n.Start + n.Length), Is.LessThanOrEqualTo(sequenceLength), "Fits in the loop.");
                Assert.That(notes.Select(n => Protocol.ProtocolNoteNumber(n.Pitch)), Is.All.InRange(lowest, highest), "On the keyboard.");
            }
        }

        [TestCaseSource(nameof(Tunes))]
        public void EveryTune_HasNoOverlappingNotesOnTheSamePitch(Tune tune)
        {
            IReadOnlyList<Tone> tones = tune.ToTones(TimeSignature.FourFour, 96);

            foreach (IGrouping<string, Tone> samePitch in tones.GroupBy(t => t.Pitch.ToString()))
            {
                List<Tone> ordered = samePitch.OrderBy(t => t.StartPosition.TotalTicks(TimeSignature.FourFour, 96)).ToList();

                for (var i = 1; i < ordered.Count; i++)
                {
                    Assert.That(ordered[i].StartPosition.TotalTicks(TimeSignature.FourFour, 96),
                        Is.GreaterThanOrEqualTo(ordered[i - 1].EndPosition.TotalTicks(TimeSignature.FourFour, 96)),
                        $"{samePitch.Key} overlaps itself");
                }
            }
        }

        [Test]
        public void OdeToJoy_StartsWithItsFamousFirstPhrase()
        {
            Tune ode = TuneLibrary.All.Single(t => t.Name == "Ode to Joy");
            IEnumerable<string> opening = TuneNotation.Parse(ode.Voices[0].Notation).Take(8).Select(n => n.Pitch.ToString());

            // E E F G G F E D, in this app's octave numbering.
            Assert.That(opening, Is.EqualTo(new[] { "E5", "E5", "F5", "G5", "G5", "F5", "E5", "D5" }));
        }
    }
}
