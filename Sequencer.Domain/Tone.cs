namespace Sequencer.Domain
{
    /// <summary>
    /// This domain has a concept of a 'Note' which doesn't include positions and velocity.
    /// This <see cref="Tone" /> class unites the concepts of <see cref="Pitch" />, <see cref="Velocity" /> and <see cref="IPosition" />,
    /// all of which make a 'tone'.
    /// </summary>
    public sealed class Tone
    {
        private IPosition endPosition;
        private Pitch pitch;
        private IPosition startPosition;
        private Velocity velocity;


        public Tone(Pitch pitch, Velocity velocity, IPosition startPosition, IPosition endPosition)
        {
            this.pitch = pitch;
            this.velocity = velocity;
            this.startPosition = startPosition;
            this.endPosition = endPosition;
        }

        public Pitch Pitch
        {
            get { return pitch; }
            set
            {
                if (value != null)
                {
                    pitch = value;
                }
            }
        }

        public Velocity Velocity
        {
            get { return velocity; }
            set
            {
                if (value != null)
                {
                    velocity = value;
                }
            }
        }

        public IPosition StartPosition
        {
            get { return startPosition; }
            set
            {
                if (value != null)
                {
                    startPosition = value;
                }
            }
        }

        public IPosition EndPosition
        {
            get { return endPosition; }
            set
            {
                if ((value != null) && (value.IsGreaterThan(StartPosition)))
                {
                    endPosition = value;
                }
            }
        }
    }
}