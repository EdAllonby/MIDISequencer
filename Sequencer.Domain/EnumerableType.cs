using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

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
        private static readonly Dictionary<int, TElement> ElementsById = new Dictionary<int, TElement>();

        static EnumerableType()
        {
            // We need to run this to make sure our static fields get instantiated before use.
            var info = typeof(TElement).GetFields(BindingFlags.Static | BindingFlags.Public | BindingFlags.DeclaredOnly);
            info[1]?.GetValue(null);
        }

        protected EnumerableType(int value, string displayName)
        {
            Value = value;
            DisplayName = displayName;

            ElementsById.Add(value, (TElement) this);
        }

        public int Value { get; }

        public string DisplayName { get; }

        public static TElement FromValue(int value)
        {
            return Parse(value, "value", item => item.Value == value);
        }

        public static TElement FromDisplayName(string displayName)
        {
            return Parse(displayName, "display name", item => item.DisplayName == displayName);
        }

        public static TElement GetNextElement(TElement element)
        {
            return ElementsById[(element.Value + 1)%ElementsById.Count];
        }

        private static TElement Parse<TValue>(TValue value, string description, Func<TElement, bool> predicate)
        {
            TElement matchingItem = ElementsById.Values.FirstOrDefault(predicate);

            if (matchingItem != null)
            {
                return matchingItem;
            }

            string message = $"'{value}' is not a valid {description} in {typeof(TElement)}";
            throw new ApplicationException(message);
        }

        public override string ToString()
        {
            return DisplayName;
        }
    }
}