using System;

namespace Sequencer.Domain
{
    public interface ILessThanOrEqualComparable<in T> : IComparable<T>
    {
        bool IsLessThan(T other);

        bool IsLessThanOrEqual(T other);
    }
}