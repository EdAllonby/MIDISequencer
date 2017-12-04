using JetBrains.Annotations;

namespace Sequencer.Domain
{
    public interface IPosition : IGreaterThanOrEqualComparable<IPosition>, ILessThanOrEqualComparable<IPosition>
    {
        int Measure { get; }

        int Bar { get; }

        int Beat { get; }

        int Ticks { get; }

        [NotNull]
        IPosition PreviousPosition([NotNull] TimeSignature timeSignature);

        [NotNull]
        IPosition NextPosition([NotNull] TimeSignature timeSignature);

        [NotNull]
        IPosition PositionRelativeByBeats(int beatDelta, [NotNull] TimeSignature timeSignature);

        int SummedBeat([NotNull] TimeSignature timeSignature);
    }
}