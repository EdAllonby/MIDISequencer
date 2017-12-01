using JetBrains.Annotations;

namespace Sequencer.Domain
{
    /// <summary>
    /// Calculates steps between <see cref="Pitch" />es.
    /// </summary>
    public static class PitchStepCalculator
    {
        private const int NotesPerOctave = 12;

        /// <summary>
        /// Find the half steps between two <see cref="Pitch" />es.
        /// </summary>
        /// <param name="firstPitch">The first pitch to set the origin.</param>
        /// <param name="secondPitch">The second pitch to find the half step difference from origin.</param>
        /// <returns>The half steps between two <see cref="Pitch" />es.</returns>
        [Pure]
        public static int FindStepsFromPitches([NotNull] Pitch firstPitch, [NotNull] Pitch secondPitch)
        {
            int firstPitchSteps = firstPitch.Note.Value + (firstPitch.Octave*NotesPerOctave);
            int secondPitchSteps = secondPitch.Note.Value + (secondPitch.Octave*NotesPerOctave);

            return secondPitchSteps - firstPitchSteps;
        }
    }
}