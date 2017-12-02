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
        [NotNull] private IPosition endPosition;


        public Tone([NotNull] Pitch pitch, [NotNull] Velocity velocity, [NotNull] IPosition startPosition, [NotNull] IPosition endPosition)
        {
            Pitch = pitch;
            Velocity = velocity;
            StartPosition = startPosition;
            this.endPosition = endPosition;
        }

        [NotNull]
        public Pitch Pitch { get; set; }

        [NotNull]
        public Velocity Velocity { get; set; }

        [NotNull]
        public IPosition StartPosition { get; set; }

        [NotNull]
        public IPosition EndPosition
        {
            get => endPosition;
            set
            {
                if (value.IsGreaterThan(StartPosition))
                {
                    endPosition = value;
                }
            }
        }
    }
}