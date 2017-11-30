using System;

namespace Sequencer.Domain
{
    public interface IGreaterThanOrEqualComparable<in T> : IComparable<T>
    {
        bool IsGreaterThan(T other);

        bool IsGreaterThanOrEqual(T other);

    }
}