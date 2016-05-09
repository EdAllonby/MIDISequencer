using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

namespace Sequencer.Tests
{
    [TestFixture]
    internal class EnumerableTests
    {
        [Test]
        public void NotContainsSomeElements()
        {
            List<int> source = new List<int> {1, 2, 3, 4};
            IEnumerable<int> subset = new List<int> {4, 2};
            var test = source.Where(subset.NotContains).ToList();

            Assert.That(test, Is.Not.SubsetOf(subset));
        }
    }
}