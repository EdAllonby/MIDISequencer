using NUnit.Framework;
using Sequencer.Domain.Settings;
using Sequencer.Midi;

namespace Sequencer.Domain.Tests
{
    [TestFixture]
    internal class MidiProtocolTests
    {
        private readonly IDigitalAudioProtocol protocol = new MidiProtocol(new PitchAndPositionCalculator(new SequencerSettings()));

        private static readonly object[] PitchMidiNumberCases =
        {
            new object[] { new Pitch(Note.C, 0), 0 },
            new object[] { new Pitch(Note.D, 2), 26 },
            new object[] { new Pitch(Note.FSharp, 9), 114 },
            new object[] { new Pitch(Note.G, 10), 127 },
            new object[] { new Pitch(Note.C, 1), 12 },
            new object[] { new Pitch(Note.B, 3), 47 },
            new object[] { new Pitch(Note.C, 4), 48 },
            new object[] { new Pitch(Note.F, 6), 77 },
            new object[] { new Pitch(Note.DSharp, 8), 99 }
        };

        [Test]
        [TestCaseSource(nameof(PitchMidiNumberCases))]
        public void AssertMidiNumberFromPitch(Pitch pitch, int expectedMidiNumber)
        {
            int midiNoteNumber = protocol.ProtocolNoteNumber(pitch);
            Assert.AreEqual(expectedMidiNumber, midiNoteNumber);
        }

        [Test]
        [TestCaseSource(nameof(PitchMidiNumberCases))]
        public void MidiNote0ShouldReturnC0(Pitch expectedPitch, int midiNumber)
        {
            Pitch actualPitch = protocol.CreatePitchFromProtocolNumber(midiNumber);
            Assert.AreEqual(expectedPitch, actualPitch);
        }
    }
}