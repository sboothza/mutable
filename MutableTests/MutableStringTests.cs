using System;
using Mutable;
using NUnit.Framework;
// ReSharper disable SpecifyACultureInStringConversionExplicitly

namespace MutableTests
{
    [TestFixture]
    public class MutableStringTests
    {
        [Test]
        public void CreateAndReturn()
        {
            var sample  = "the quick brown fox";
            var mutable = new MutableString(sample);
            var result  = mutable.ToString();
            Assert.AreEqual(sample, result);
        }

        [Test]
        public void Append()
        {
            var mutable = new MutableString("the quick brown");
            mutable.Append(" fox");
            var result = mutable.ToString();
            Assert.AreEqual("the quick brown fox", result);
        }

        [Test]
        public void Prepend()
        {
            var mutable = new MutableString("quick brown fox");
            mutable.Prepend("the ");
            var result = mutable.ToString();
            Assert.AreEqual("the quick brown fox", result);
        }

        [Test]
        public void Substring()
        {
            var mutable = new MutableString("the quick brown fox");
            var sub     = mutable.GetSubString(10, 5);
            Assert.AreEqual("brown", sub);
            mutable.SetSubString(10, "green");
            var result = mutable.ToString();
            Assert.AreEqual("the quick green fox", result);
        }

        [Test]
        public void Index()
        {
            var mutable = new MutableString("the quick brown fox");
            var c       = mutable[2];
            Assert.AreEqual('e', c);
            mutable[2] = 'E';
            var result = mutable.ToString();
            Assert.AreEqual("thE quick brown fox", result);
        }

        [Test]
        public void Insert()
        {
            var mutable = new MutableString("the brown fox");
            mutable.Insert(4, "quick ");
            var result = mutable.ToString();
            Assert.AreEqual("the quick brown fox", result);
        }

        [Test]
        public void Search()
        {
            var mutable  = new MutableString("the quick brown fox quickly ate the rat");
            var position = mutable.IndexOf("quick");
            Assert.AreEqual(4, position);

            position = mutable.IndexOf("quick", 5);
            Assert.AreEqual(20, position);

            position = mutable.IndexOf("quick", 21);
            Assert.AreEqual(-1, position);

            mutable  = new MutableString("the qqqqqquick brown fox quickly ate the rat");
            position = mutable.IndexOf("qqquick");
            Assert.AreEqual(7, position);
        }

        [Test]
        public void Comparisons()
        {
            var str1 = new MutableString("the quick brown fox");
            var str2 = new MutableString("the quick brown fox quickly ate the rat");
            var str3 = new MutableString("quick");
            var str4 = new MutableString("the quick brown fox");

            var val = str1.CompareTo(str2);
            Assert.Less(val, 0);
            val = str2.CompareTo(str1);
            Assert.Greater(val, 0);

            val = str1.CompareTo(str3);
            Assert.Greater(val, 0);
            val = str3.CompareTo(str1);
            Assert.Less(val, 0);

            val = str2.CompareTo(str3);
            Assert.Greater(val, 0);
            val = str3.CompareTo(str2);
            Assert.Less(val, 0);

            val = str1.CompareTo(str4);
            Assert.AreEqual(0, val);

            Assert.True(str1 == str4);
            string v = str1;
            var    c = (char[])str1;
            Assert.AreEqual(str1.ToString(), v);
            Assert.AreEqual(str1.Length, c.Length);
        }

        [Test]
        public void Replace()
        {
            var str1 = new MutableString("the quick brown fox quickly ate the rat");
            var str2 = str1.Clone() as MutableString;

            str1.Replace("fox", "cow");
            Assert.AreEqual("the quick brown cow quickly ate the rat", str1.ToString());

            str2?.Replace("quick", "slow");
            Assert.AreEqual("the slow brown fox slowly ate the rat", str2?.ToString());

            str2.Replace("the", "__THE__");
            Assert.AreEqual("__THE__ slow brown fox slowly ate __THE__ rat", str2.ToString());
        }

        [Test]
        public void Trim()
        {
            var str = new MutableString("   the quick brown fox quickly ate the rat   ");
            str.TrimStart();
            Assert.AreEqual("the quick brown fox quickly ate the rat   ", str.ToString());

            str.TrimEnd();
            Assert.AreEqual("the quick brown fox quickly ate the rat", str.ToString());

            str = new MutableString("   the quick brown fox quickly ate the rat   ");
            str.Trim();
            Assert.AreEqual("the quick brown fox quickly ate the rat", str.ToString());
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
            var source = "the quick brown fox quickly ate the rat".ToMutable();
            var sought = "quick".ToMutable();

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
    }
}