using System;
using JetBrains.Annotations;

namespace Sequencer.Domain
{
    public class FrequencyCalculator : IFrequencyCalculator
    {
        private const double TwelthRootOfTwo = 1.05946309;
        [NotNull] private readonly Pitch originPitch;
        [NotNull] private readonly IPitchAndPositionCalculator pitchAndPositionCalculator;
        private readonly double standardFrequency;

        public FrequencyCalculator([NotNull] IPitchAndPositionCalculator pitchAndPositionCalculator)
        {
            this.pitchAndPositionCalculator = pitchAndPositionCalculator;
            originPitch = new Pitch(Note.A, 4);
            standardFrequency = 440;
        }

        /// <summary>
        /// Gets the frequency of a pitch in hertz.
        /// </summary>
        /// <param name="pitch">The pitch to find the frequency of.</param>
        /// <returns>the frequency of the given pitch in hertz.</returns>
        public double PitchFrequency(Pitch pitch)
        {
            int halfStepsFromOrigin = pitchAndPositionCalculator.FindStepsFromPitches(originPitch, pitch);

            return standardFrequency * Math.Pow(TwelthRootOfTwo, halfStepsFromOrigin);
        }
    }
}