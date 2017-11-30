namespace Sequencer.Domain
{
    public interface IPosition : IGreaterThanOrEqualComparable<IPosition>, ILessThanOrEqualComparable<IPosition>
    {
        int Measure { get; }

        int Bar { get; }

        int Beat { get; }
        IPosition PreviousPosition(TimeSignature timeSignature);

        IPosition NextPosition(TimeSignature timeSignature);
        IPosition PositionRelativeByBeats(int beatDelta, TimeSignature timeSignature);

        int SummedBeat(TimeSignature timeSignature);
    }
}