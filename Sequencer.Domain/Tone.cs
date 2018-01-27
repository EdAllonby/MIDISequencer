using JetBrains.Annotations;

namespace Sequencer.Domain
{
    /// <summary>
    /// This domain has a concept of a 'Note' which doesn't include positions and velocity.
    /// This <see cref="Tone" /> class unites the concepts of <see cref="Pitch" />, <see cref="Velocity" /> and
    /// <see cref="IPosition" />,
    /// all of which make a 'tone'.
    /// </summary>
    public sealed class Tone
    {
        public Tone([NotNull] Pitch pitch, [NotNull] Velocity velocity, [NotNull] IPosition startPosition, [NotNull] IPosition endPosition)
        {
            Pitch = pitch;
            Velocity = velocity;
            StartPosition = startPosition;
            EndPosition = endPosition.IsGreaterThan(StartPosition) ? endPosition : startPosition;
        }

        [NotNull]
        public Pitch Pitch { get; }

        [NotNull]
        public Velocity Velocity { get; }

        [NotNull]
        public IPosition StartPosition { get; }

        [NotNull]
        public IPosition EndPosition { get; }
    }
}