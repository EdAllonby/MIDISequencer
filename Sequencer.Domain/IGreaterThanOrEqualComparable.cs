using System;
using JetBrains.Annotations;

namespace Sequencer.Domain
{
    public interface IGreaterThanOrEqualComparable<in T> : IComparable<T>
    {
        bool IsGreaterThan([NotNull] T other);

        bool IsGreaterThanOrEqual([NotNull] T other);
    }
}