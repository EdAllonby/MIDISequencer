using NUnit.Framework;

namespace Sequencer.Tests
{
    [TestFixture]
    internal class PitchTests
    {
        [Test]
        public void C0ShouldReturnMidiNote0()
        {
            var pitch = new Pitch(Note.C, 0);
            Assert.AreEqual(pitch.MidiNoteNumber, 0);
        }

        [Test]
        public void D2ShouldReturnMidiNote26()
        {
            var pitch = new Pitch(Note.D, 2);
            Assert.AreEqual(pitch.MidiNoteNumber, 26);
        }

        [Test]
        public void FSharp9ShouldReturnMidiNote114()
        {
            var pitch = new Pitch(Note.FSharp, 9);
            Assert.AreEqual(pitch.MidiNoteNumber, 114);
        }

        [Test]
        public void MidiNote12ShouldReturnC1()
        {
            var pitch = Pitch.CreatePitchFromMidiNumber(12);
            var expectedPitch = new Pitch(Note.C, 1);
            Assert.AreEqual(expectedPitch, pitch);
        }

        [Test]
        public void MidiNote47ShouldReturnB3()
        {
            var pitch = Pitch.CreatePitchFromMidiNumber(47);
            var expectedPitch = new Pitch(Note.B, 3);
            Assert.AreEqual(expectedPitch, pitch);
        }

        [Test]
        public void MidiNote77ShouldReturnF6()
        {
            var pitch = Pitch.CreatePitchFromMidiNumber(77);
            var expectedPitch = new Pitch(Note.F, 6);
            Assert.AreEqual(expectedPitch, pitch);
        }

        [Test]
        public void MidiNote127ShouldReturnG10()
        {
            var pitch = Pitch.CreatePitchFromMidiNumber(127);
            var expectedPitch = new Pitch(Note.G, 10);
            Assert.AreEqual(expectedPitch, pitch);
        }

        [Test]
        public void MidiNote0ShouldReturnC0()
        {
            var pitch = Pitch.CreatePitchFromMidiNumber(0);
            var expectedPitch = new Pitch(Note.C, 0);
            Assert.AreEqual(expectedPitch, pitch);
        }


        [Test]
        public void MidiNote48ShouldReturnC4()
        {
            var pitch = Pitch.CreatePitchFromMidiNumber(48);
            var expectedPitch = new Pitch(Note.C, 4);
            Assert.AreEqual(expectedPitch, pitch);
        }
        [Test]
        public void MidiNote99ShouldReturnDSharp8()
        {
            var pitch = Pitch.CreatePitchFromMidiNumber(99);
            var expectedPitch = new Pitch(Note.DSharp, 8);
            Assert.AreEqual(expectedPitch, pitch);
        }
    }
}