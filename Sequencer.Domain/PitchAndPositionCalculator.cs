using JetBrains.Annotations;

namespace Sequencer.Domain
{
    /// <summary>
    /// Calculates steps between <see cref="Pitch" />es and beats between <see cref="IPosition" />s.
    /// </summary>
    public class PitchAndPositionCalculator : IPitchAndPositionCalculator
    {
        private const int NotesPerOctave = 12;
        [NotNull] private readonly TimeSignature timeSignature;

        public PitchAndPositionCalculator()
        {
            // TODO: Inject
            this.timeSignature = new TimeSignature(4,4);
        }

        /// <summary>
        /// Find the half steps between two <see cref="Pitch" />es.
        /// </summary>
        /// <param name="firstPitch">The first pitch to set the origin.</param>
        /// <param name="secondPitch">The second pitch to find the half step difference from origin.</param>
        /// <returns>The half steps between two <see cref="Pitch" />es.</returns>
        [Pure]
        public int FindStepsFromPitches(Pitch firstPitch, Pitch secondPitch)
        {
            int firstPitchSteps = firstPitch.Note.Value + firstPitch.Octave * NotesPerOctave;
            int secondPitchSteps = secondPitch.Note.Value + secondPitch.Octave * NotesPerOctave;

            return secondPitchSteps - firstPitchSteps;
        }

        /// <summary>
        /// Finds the beats between two <see cref="IPosition" />s.
        /// </summary>
        /// <param name="initialPosition">The first position to set the origin.</param>
        /// <param name="newPosition">The second position to calculate the difference from origin.</param>
        /// <returns>The different in beats between two positions.</returns>
        [Pure]
        public int FindBeatsBetweenPositions(IPosition initialPosition, IPosition newPosition)
        {
            return newPosition.SummedBeat(timeSignature) - initialPosition.SummedBeat(timeSignature);
        }
    }
}