using System;
using JetBrains.Annotations;

namespace Sequencer.Domain
{
    public interface ILessThanOrEqualComparable<in T> : IComparable<T>
    {
        bool IsLessThan([NotNull] T other);

        bool IsLessThanOrEqual([NotNull] T other);
    }
}