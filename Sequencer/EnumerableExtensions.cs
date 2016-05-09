using System;
using System.Collections.Generic;
using System.Linq;

namespace Sequencer
{
    public static class EnumerableExtensions
    {
        /// <summary>
        /// Wraps this object instance into an <see cref="IEnumerable{T}" /> consisting of a single item.
        /// </summary>
        /// <typeparam name="T"> Type of the object. </typeparam>
        /// <param name="item"> The instance that will be wrapped. </param>
        /// <returns> An <see cref="IEnumerable{T}" /> consisting of a single item. </returns>
        public static IEnumerable<T> Yield<T>(this T item)
        {
            yield return item;
        }

        /// <summary>
        /// Checks if a source collection doesn't contain a value.
        /// </summary>
        /// <typeparam name="TSource">The type of items in the source collection.</typeparam>
        /// <param name="source">The source collection to check contents.</param>
        /// <param name="value">The value to check if not contained in source.</param>
        /// <returns>If the value is not contained in source.</returns>
        public static bool NotContains<TSource>(this IEnumerable<TSource> source, TSource value)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            EqualityComparer<TSource> comparer = EqualityComparer<TSource>.Default;
            
            return source.All(element => !comparer.Equals(element, value));
        }
    }
}