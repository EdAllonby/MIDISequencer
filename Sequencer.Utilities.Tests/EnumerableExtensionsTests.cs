using System.Collections.Generic;
using NUnit.Framework;

namespace Sequencer.Utilities.Tests
{
    [TestFixture]
    public class EnumerableExtensionsTests
    {
        private static readonly object[] YieldCases =
        {
            new object[] {0, new List<int> {0}},
            new object[] {"hello", new List<string> {"hello"}},
            new object[] {new List<int> {1}, new List<List<int>> {new List<int> {1}}}
        };

        [Test]
        [TestCaseSource(nameof(YieldCases))]
        public void YieldTests<T>(T thing, IEnumerable<T> expected)
        {
            IEnumerable<T> actual = thing.Yield();

            Assert.AreEqual(expected, actual);
        }
    }
}