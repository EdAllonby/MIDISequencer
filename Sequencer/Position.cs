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

        public int SummedBeat(int barsPerMeasure, int beatsPerBar)
        {
            return ((Measure - 1)*barsPerMeasure*beatsPerBar) + ((Bar - 1)*beatsPerBar) + Beat;
        }
    }
}