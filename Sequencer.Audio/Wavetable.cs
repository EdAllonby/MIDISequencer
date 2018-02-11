using System.Collections.Generic;
using JetBrains.Annotations;

namespace Sequencer.Audio
{
    public sealed class Wavetable
    {
        [NotNull] private readonly List<float> samples;

        public Wavetable([NotNull] List<float> wavetableSamples)
        {
            samples = wavetableSamples;
        }

        public int Size => samples.Count;

        public float SampleAtPosition(int position)
        {
            int wrappedPosition = position % samples.Count;

            return samples[wrappedPosition];
        }
    }
}