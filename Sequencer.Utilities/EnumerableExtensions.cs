using System.Collections.Generic;
using JetBrains.Annotations;

namespace Sequencer.Utilities
{
    public static class EnumerableExtensions
    {
        /// <summary>
        /// Wraps this object instance into an <see cref="IEnumerable{T}" /> consisting of a single item.
        /// </summary>
        /// <typeparam name="T"> Type of the object. </typeparam>
        /// <param name="item"> The instance that will be wrapped. </param>
        /// <returns> An <see cref="IEnumerable{T}" /> consisting of a single item. </returns>
        [NotNull]
        public static IEnumerable<T> Yield<T>([NotNull] this T item)
        {
            yield return item;
        }
    }
}