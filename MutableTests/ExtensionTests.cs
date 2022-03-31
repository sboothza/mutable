using System;
using System.Diagnostics;
using Mutable;
using NUnit.Framework;

namespace MutableTests
{
    [TestFixture]
    public class ExtensionTests
    {
        [Test]
        public void StringTests()
        {
            var s1 = "string1" + "string2";

            //var s2 = s1        - "string2";

            Assert.True(true);
        }

        [Test]
        public void ArrayTests()
        {
            var source  = "the quick brown fox".ToCharArray();
            var source2 = source.Clone() as char[];
            Assert.AreEqual(source, source2);
        }

        [Test]
        public void BasicFind()
        {
            var source = "the quick brown fox".ToCharArray();
            var sought = "brown".ToCharArray();

            var position = source.SearchInCharArray(sought);
            Assert.AreEqual(10, position);

            position = source.ReverseSearchInCharArray(sought);
            Assert.AreEqual(10, position);
        }

        [Test]
        public void AdvancedFind()
        {
            var source = "the quick brown fox quickly ate the rat".ToCharArray();
            var sought = "quick".ToCharArray();

            var position = source.SearchInCharArray(sought);
            Assert.AreEqual(4, position);

            position = source.ReverseSearchInCharArray(sought);
            Assert.AreEqual(20, position);

            position = source.SearchInCharArray(sought, 5);
            Assert.AreEqual(20, position);

            position = source.ReverseSearchInCharArray(sought, 20);
            Assert.AreEqual(4, position);

            position = source.SearchInCharArray(sought, 21);
            Assert.AreEqual(-1, position);

            position = source.ReverseSearchInCharArray(sought, 4);
            Assert.AreEqual(-1, position);
        }

        [Test]
        public void SpecialCase()
        {
            var source = "the qqqqqquick brown fox quickly ate the rat".ToCharArray();
            var sought = "quick".ToCharArray();

            var position = source.SearchInCharArray(sought);
            Assert.AreEqual(9, position);

            position = source.ReverseSearchInCharArray(sought);
            Assert.AreEqual(25, position);

            sought   = "qqquick".ToCharArray();
            position = source.SearchInCharArray(sought);
            Assert.AreEqual(7, position);

            position = source.ReverseSearchInCharArray(sought);
            Assert.AreEqual(7, position);
        }

        [Test]
        [Explicit]
        public void PerformanceStandard()
        {
            var source = "the quick brown fox quickly ate the rat";
            var sought = "quick";

            var startTime = DateTime.Now;

            for (var i = 0; i < 10000000; i++)
            {
                var position = source.IndexOf(sought, 5);
                if (position != 20)
                    throw new InvalidOperationException("bugger");
            }

            var duration = DateTime.Now - startTime;
            Console.WriteLine($"total:{duration:c}");
        }

        [Test]
        [Explicit]
        public void PerformanceCustom()
        {
            var source = "the quick brown fox quickly ate the rat".ToCharArray();
            var sought = "quick".ToCharArray();

            var startTime = DateTime.Now;

            for (var i = 0; i < 10000000; i++)
            {
                var position = source.SearchInCharArray(sought, 5);
                if (position != 20)
                    throw new InvalidOperationException("bugger");
            }

            var duration = DateTime.Now - startTime;
            Console.WriteLine($"total:{duration:c}");
        }

        [Test]
        public void Compare()
        {
            var str1 = "the quick brown fox".ToCharArray();
            var str2 = "the quick brown fox quickly ate the rat".ToCharArray();
            var str3 = "quick".ToCharArray();
            var str4 = "the quick brown fox".ToCharArray();

            var val = str1.CompareCharArray(str2);
            Assert.Less(val, 0);
            val = str2.CompareCharArray(str1);
            Assert.Greater(val, 0);

            val = str1.CompareCharArray(str3);
            Assert.Greater(val, 0);
            val = str3.CompareCharArray(str1);
            Assert.Less(val, 0);

            val = str2.CompareCharArray(str3);
            Assert.Greater(val, 0);
            val = str3.CompareCharArray(str2);
            Assert.Less(val, 0);

            val = str1.CompareCharArray(str4);
            Assert.AreEqual(0, val);
        }

        [Test]
        public void TestToMutable()
        {
            var strings = new[] { "one", "two", "three" };
            foreach (var ms in strings.ToMutable())
                Trace.WriteLine($"{ms}");
        }

        [Test]
        public void TestSplit()
        {
            foreach (var s in "this is a test of my happiness".ToCharArray().Split(' '))
                Trace.WriteLine(new string(s));
        }
    }
}