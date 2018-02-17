using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;

namespace Sequencer.Domain
{
    /// <summary>
    /// Inspired from Jimmy Boggard's post on enumeration classes:
    /// https://lostechies.com/jimmybogard/2008/08/12/enumeration-classes/
    /// This is a replacement for Enums, where more is required, such as displayable names and better parsing.
    /// </summary>
    /// <typeparam name="TElement">The enumerable element.</typeparam>
    public abstract class EnumerableType<TElement> where TElement : EnumerableType<TElement>
    {
        [NotNull] private static readonly Dictionary<int, TElement> ElementsById = new Dictionary<int, TElement>();

        static EnumerableType()
        {
            // We need to run this to make sure our static fields get instantiated before use.
            FieldInfo[] info = typeof(TElement).GetFields(BindingFlags.Static | BindingFlags.Public | BindingFlags.DeclaredOnly);
            info[1]?.GetValue(null);
        }

        protected EnumerableType(int value, [NotNull] string displayName)
        {
            Value = value;
            DisplayName = displayName;

            if (ElementsById.ContainsKey(value))
            {
                throw new ArgumentException($"Value {value} already exists for element {typeof(TElement)}.");
            }

            ElementsById.Add(value, (TElement) this);
        }

        public int Value { get; }

        [NotNull]
        public string DisplayName { get; }

        [NotNull]
        public static IEnumerable<TElement> All => ElementsById.Values;

        public static int Count() => Count(x => true);

        public static int Count([NotNull] Func<TElement, bool> predicate)
        {
            return All.Where(predicate).Count();
        }

        [NotNull]
        public static TElement FromValue(int value)
        {
            return Parse(value, "value", item => item?.Value == value);
        }

        public static TElement FromDisplayName([NotNull] string displayName)
        {
            return Parse(displayName, "display name", item => item?.DisplayName == displayName);
        }

        [NotNull]
        public static TElement GetNextElement([NotNull] TElement element)
        {
            return ElementsById[(element.Value + 1) % ElementsById.Count] ?? throw new InvalidOperationException();
        }

        public override string ToString()
        {
            return DisplayName;
        }

        [NotNull]
        private static TElement Parse<TValue>([NotNull] TValue value, [NotNull] string description, [NotNull] Func<TElement, bool> predicate)
        {
            TElement matchingItem = ElementsById.Values.FirstOrDefault(predicate);

            if (matchingItem != null)
            {
                return matchingItem;
            }

            string message = $"'{value}' is not a valid {description} in {typeof(TElement)}";
            throw new ApplicationException(message);
        }
    }
}