using System;

namespace Sequencer.Domain
{
    public class FrequencyCalculator
    {
        private const double TwelthRootOfTwo = 1.05946309;
        private readonly Pitch originPitch;
        private readonly double standardFrequency;


        public FrequencyCalculator(Pitch originPitch, double standardFrequency)
        {
            this.originPitch = originPitch;
            this.standardFrequency = standardFrequency;
        }

        /// <summary>
        /// Gets the frequency of a pitch in hertz.
        /// </summary>
        /// <param name="pitch">The pitch to find the frequency of.</param>
        /// <returns>the frequency of the given pitch in hertz.</returns>
        public double PitchFrequency(Pitch pitch)
        {
            int halfStepsFromOrigin = PitchStepCalculator.FindStepsFromPitches(originPitch, pitch);

            return standardFrequency*Math.Pow(TwelthRootOfTwo, halfStepsFromOrigin);
        }
    }
}