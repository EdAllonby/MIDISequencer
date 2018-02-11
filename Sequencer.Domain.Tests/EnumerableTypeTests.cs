using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using NUnit.Framework;

namespace Sequencer.Domain.Tests
{
    [TestFixture]
    public class EnumerableTypeTests
    {
        private class EnumerableTypeTestClass : EnumerableType<EnumerableTypeTestClass>
        {
            [UsedImplicitly] [NotNull] public static readonly EnumerableTypeTestClass EnumerableType1 = new EnumerableTypeTestClass(0, "EnumerableType1");
            [UsedImplicitly] [NotNull] public static readonly EnumerableTypeTestClass EnumerableType2 = new EnumerableTypeTestClass(1, "EnumerableType2");
            [UsedImplicitly] [NotNull] public static readonly EnumerableTypeTestClass EnumerableType3 = new EnumerableTypeTestClass(2, "EnumerableType3");
            [UsedImplicitly] [NotNull] public static readonly EnumerableTypeTestClass EnumerableType4 = new EnumerableTypeTestClass(3, "EnumerableType4");

            private EnumerableTypeTestClass(int value, string displayName) : base(value, displayName)
            {
            }
        }

        [Test]
        public void EnumerableTypeAllTest()
        {
            IEnumerable<EnumerableTypeTestClass> all = EnumerableTypeTestClass.All;

            var expected = new List<EnumerableTypeTestClass>
            {
                EnumerableTypeTestClass.EnumerableType1,
                EnumerableTypeTestClass.EnumerableType2,
                EnumerableTypeTestClass.EnumerableType3,
                EnumerableTypeTestClass.EnumerableType4
            };

            CollectionAssert.AreEquivalent(expected, all);
        }

        [Test]
        public void EnumerableTypeCountTest()
        {
            int count = EnumerableTypeTestClass.Count();

            Assert.AreEqual(4, count);
        }

        [Test]
        public void EnumerableTypeCountPredicateTest()
        {
            int count = EnumerableTypeTestClass.Count(x => x.Value % 2 == 0);

            Assert.AreEqual(2, count);
        }

        [Test]
        public void FromDisplayName_WhenNoneExists_ThrowsApplicationException()
        {
            Assert.Throws<ApplicationException>(() => EnumerableTypeTestClass.FromDisplayName("unexpected"));
        }

        [Test]
        public void FromValue_WhenNoneExists_ThrowsApplicationException()
        {
            Assert.Throws<ApplicationException>(() => EnumerableTypeTestClass.FromValue(-1));
        }
    }
}