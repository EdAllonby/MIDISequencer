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
        IPosition PreviousPosition(NoteResolution resolution, [NotNull] TimeSignature timeSignature, int ticksPerQuarterNote);

        [NotNull]
        IPosition NextPosition(NoteResolution resolution, [NotNull] TimeSignature timeSignature, int ticksPerQuarterNote);

        [NotNull]
        IPosition PositionRelativeByTicks(int tickDelta, [NotNull] TimeSignature timeSignature, int ticksPerQuarterNote);

        int SummedBeat([NotNull] TimeSignature timeSignature);

        int TotalTicks([NotNull] TimeSignature timeSignature, int ticksPerQuarterNote);
    }
}