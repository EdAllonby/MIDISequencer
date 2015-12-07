namespace Sequencer
{
    public sealed class Position
    {
        public Position(int measure, int bar, int beat)
        {
            Measure = measure;
            Bar = bar;
            Beat = beat;
        }

        private int Measure { get; }

        private int Bar { get; }

        private int Beat { get; }

        public int SummedBeat(TimeSignature timeSignature)
        {
            return ((Measure - 1)* timeSignature.BeatsPerMeasure) + ((Bar - 1)* timeSignature.BeatsPerBar) + Beat;
        }

        public override string ToString()
        {
            return string.Format("Position at Measure {0}, Bar {1}, Beat {2}", Measure, Bar, Beat);
        }
    }
}