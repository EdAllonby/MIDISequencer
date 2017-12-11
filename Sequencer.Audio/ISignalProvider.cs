using JetBrains.Annotations;
using NAudio.Wave;

namespace Sequencer.Audio
{
    public interface ISignalProvider : IWaveProvider
    {
        float Amplitude { get; set; }
        float Frequency { get; set; }
        bool NoteState { get; set; }

        int Read([NotNull] float[] buffer, int offset, int sampleCount);
    }
}