namespace Sequencer.Domain
{
    /// <summary>
    /// This domain has a concept of a 'Note' which doesn't include positions and velocity.
    /// This <see cref="Tone" /> class unites the concepts of <see cref="Pitch" />, <see cref="Velocity" /> and <see cref="Position" />,
    /// all of which make a 'tone'.
    /// </summary>
    public sealed class Tone
    {
        private Position endPosition;
        private Pitch pitch;
        private Position startPosition;
        private Velocity velocity;


        public Tone(Pitch pitch, Velocity velocity, Position startPosition, Position endPosition)
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

        public Position StartPosition
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

        public Position EndPosition
        {
            get { return endPosition; }
            set
            {
                if ((value != null) && (value > StartPosition))
                {
                    endPosition = value;
                }
            }
        }
    }
}