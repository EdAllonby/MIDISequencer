using NAudio.Wave;

namespace Sequencer.Audio
{
    /// <summary>
    /// One sounding note (a voice). The mixer reads it on the audio thread until it reports finished.
    /// </summary>
    public interface ISignalProvider : ISampleProvider
    {
        float Amplitude { get; set; }

        float Frequency { get; set; }

        /// <summary>
        /// True once the note has been released and its release stage has faded to silence.
        /// </summary>
        bool IsFinished { get; }

        /// <summary>
        /// Starts the envelope's attack stage. Call before the voice is handed to the mixer.
        /// </summary>
        void NoteOn();

        /// <summary>
        /// Starts the envelope's release stage. Safe to call from any thread.
        /// </summary>
        void NoteOff();
    }
}
