using JetBrains.Annotations;
using Sequencer.Domain;

namespace Sequencer.Midi
{
    /// <summary>
    /// MIDI is a technical standard which allows a wide variety of electronic musical instruments,
    /// computers and other related devices to connect and communicate with one another.
    /// </summary>
    public sealed class MidiProtocol : IDigitalAudioProtocol
    {
        private const int NotesPerOctave = 12;
        [NotNull] private static readonly Pitch LowestMidiNote = new Pitch(Note.C, 0);
        [NotNull] private readonly IPitchAndPositionCalculator pitchAndPositionCalculator;

        public MidiProtocol([NotNull] IPitchAndPositionCalculator pitchAndPositionCalculator)
        {
            this.pitchAndPositionCalculator = pitchAndPositionCalculator;
        }

        /// <summary>
        /// The MIDI equivalent of this pitch.
        /// </summary>
        public int ProtocolNoteNumber(Pitch pitch)
        {
            return pitchAndPositionCalculator.FindStepsFromPitches(LowestMidiNote, pitch);
        }

        /// <summary>
        /// Creates a pitch from the Midi protocol number.
        /// </summary>
        /// <param name="midiValue">The Midi value to find the pitch for.</param>
        /// <returns>The pitch from the Midi value.</returns>
        public Pitch CreatePitchFromProtocolNumber(int midiValue)
        {
            int noteValue = midiValue % NotesPerOctave;

            int octave = (midiValue - noteValue) / NotesPerOctave;

            Note note = Note.FromValue(noteValue);

            return new Pitch(note, octave);
        }
    }
}