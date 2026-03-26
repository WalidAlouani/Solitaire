using NUnit.Framework;
using Solitaire.Extensions;
using System.Collections.Generic;
using System.Linq;

namespace Solitaire.Tests
{
    public class ListExtensionsTests
    {
        [Test]
        public void Shuffle_PreservesAllElements()
        {
            var list = new List<int> { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };
            var original = new List<int>(list);

            list.Shuffle();

            Assert.AreEqual(original.Count, list.Count);
            foreach (var item in original)
            {
                Assert.IsTrue(list.Contains(item), $"Shuffled list missing element {item}.");
            }
        }

        [Test]
        public void Shuffle_PreservesCount()
        {
            var list = new List<string> { "a", "b", "c", "d", "e" };
            list.Shuffle();
            Assert.AreEqual(5, list.Count);
        }

        [Test]
        public void Shuffle_EmptyList_DoesNotThrow()
        {
            var list = new List<int>();
            Assert.DoesNotThrow(() => list.Shuffle());
            Assert.AreEqual(0, list.Count);
        }

        [Test]
        public void Shuffle_SingleElement_DoesNotThrow()
        {
            var list = new List<int> { 42 };
            Assert.DoesNotThrow(() => list.Shuffle());
            Assert.AreEqual(1, list.Count);
            Assert.AreEqual(42, list[0]);
        }

        [Test]
        public void Shuffle_ProducesVariedResults()
        {
            // Run shuffle multiple times and verify it doesn't always return the same order.
            // With 10 elements, the probability of getting the exact same order is 1/10! ≈ 0.00003%
            var original = new List<int> { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };
            bool foundDifferent = false;

            for (int attempt = 0; attempt < 10; attempt++)
            {
                var list = new List<int>(original);
                list.Shuffle();

                if (!list.SequenceEqual(original))
                {
                    foundDifferent = true;
                    break;
                }
            }

            Assert.IsTrue(foundDifferent, "Shuffle should produce a different order (ran 10 attempts).");
        }

        [Test]
        public void Shuffle_NoDuplicatesIntroduced()
        {
            var list = new List<int> { 1, 2, 3, 4, 5 };
            list.Shuffle();

            var distinct = new HashSet<int>(list);
            Assert.AreEqual(list.Count, distinct.Count, "Shuffle should not introduce duplicates.");
        }
    }
}
