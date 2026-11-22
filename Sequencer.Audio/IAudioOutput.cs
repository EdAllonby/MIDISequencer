using System;
using JetBrains.Annotations;
using NAudio.Wave;

namespace Sequencer.Audio
{
    /// <summary>
    /// Sends a stream of samples to the speakers.
    /// </summary>
    public interface IAudioOutput : IDisposable
    {
        /// <summary>
        /// Starts pulling samples from <paramref name="source"/> on the audio thread until disposed.
        /// </summary>
        void Play([NotNull] ISampleProvider source);
    }
}
