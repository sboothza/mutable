using System;
using System.Diagnostics;
using System.Threading;
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

            var position = source.SearchInCharArray(0, sought, 0);
            Assert.AreEqual(10, position);

            position = source.ReverseSearchInCharArray(sought);
            Assert.AreEqual(10, position);
        }

        [Test]
        public void TestPointers()
        {
            unsafe
            {
                byte[]  byteArray = { 1, 2, 3, 4 };
                int[]   intArray  = { 1, 2, 3, 4 };
                ulong[] longArray = { 1, 2, 3, 4 };

                Trace.WriteLine("Test byte");
                fixed (byte* arr = byteArray)
                {
                    var ptr   = arr;
                    var count = byteArray.Length;
                    while (count-- > 0)
                        Trace.WriteLine($"{*ptr++}");
                }

                Trace.WriteLine("Test int");
                fixed (int* arr = intArray)
                {
                    var ptr   = arr;
                    var count = intArray.Length;
                    while (count-- > 0)
                        Trace.WriteLine($"{*ptr++}");
                }

                Trace.WriteLine("Test long");
                fixed (ulong* arr = longArray)
                {
                    var ptr   = arr;
                    var count = longArray.Length;
                    while (count-- > 0)
                        Trace.WriteLine($"{*ptr++}");
                }
            }
        }

        [Test]
        public void TestCast()
        {
            unsafe
            {
                var str1 = "the quick brown fox".ToCharArray();
                var str2 = "I am the quick brown fox".ToCharArray();

                fixed (char* s1Ptr = &str1[0], s2Ptr = &str2[5])
                {
                    Trace.WriteLine("byte");
                    var s1    = (byte*)s1Ptr;
                    var s2    = (byte*)s2Ptr;
                    var count = str1.Length * sizeof(char);
                    while (count-- > 0)
                        Trace.WriteLine($"{*s1++} - {*s2++}");

                    Trace.WriteLine("int");
                    var s1Int = (int*)s1Ptr;
                    var s2Int = (int*)s2Ptr;
                    count = str1.Length * sizeof(char) / sizeof(int);
                    while (count-- > 0)
                        Trace.WriteLine($"{*s1Int++} - {*s2Int++}");

                    Trace.WriteLine("long");
                    var s1Long = (ulong*)s1Ptr;
                    var s2Long = (ulong*)s2Ptr;
                    count = str1.Length * sizeof(char) / sizeof(ulong);
                    while (count-- > 0)
                        Trace.WriteLine($"{*s1Long++} - {*s2Long++}");
                }
            }
        }

        [Test]
        public void Teststrncmp()
        {
            var source = "the quick brown fox".ToCharArray();
            var sought = "brown".ToCharArray();

            //var sought = "the".ToCharArray();
            var result = Extensions.CompareCharArray(source, 0, sought, 0, sought.Length);
            Assert.NotZero(result);
            result = Extensions.CompareCharArray(source, 10, sought, 0, sought.Length);
            Assert.Zero(result);
        }

        [Test]
        public void BasicFindNew()
        {
            var source = "the quick brown fox".ToCharArray();
            var sought = "brown".ToCharArray();

            var position = source.search(sought);
            Assert.AreEqual(10, position);
        }

        [Test]
        public void AdvancedFind()
        {
            var source = "the quick brown fox quickly ate the rat".ToCharArray();
            var sought = "quick".ToCharArray();

            var position = source.SearchInCharArray(0, sought, 0);
            Assert.AreEqual(4, position);

            position = source.ReverseSearchInCharArray(sought);
            Assert.AreEqual(20, position);

            position = source.SearchInCharArray(5, sought, 0);
            Assert.AreEqual(20, position);

            position = source.ReverseSearchInCharArray(sought, 20);
            Assert.AreEqual(4, position);

            position = source.SearchInCharArray(21, sought, 0);
            Assert.AreEqual(-1, position);

            position = source.ReverseSearchInCharArray(sought, 4);
            Assert.AreEqual(-1, position);
        }

        [Test]
        public void TestArrayCopy()
        {
            var str1 = "the quick brown fox quickly ate the rat".ToCharArray();
            var str2 = "cow".ToCharArray();

            str2.Copy(0, str1, 16, str2.Length);
            Assert.AreEqual("the quick brown cow quickly ate the rat", new string(str1));
        }

        [Test, Explicit]
        public void PerformanceArrayCopyStandard()
        {
            var str1 = "the quick brown fox quickly ate the rat".ToCharArray();
            var str2 = "cow".ToCharArray();

            var startTime = DateTime.Now;

            for (var i = 0; i < 10000000; i++)
            {
                Array.Copy(str2, 0, str1, 16, str2.Length);
            }

            var duration = DateTime.Now - startTime;
            Console.WriteLine($"total:{duration:c}");
        }

        [Test, Explicit]
        public void PerformanceArrayCopyCustom()
        {
            var str1 = "the quick brown fox quickly ate the rat".ToCharArray();
            var str2 = "cow".ToCharArray();

            var startTime = DateTime.Now;

            for (var i = 0; i < 10000000; i++)
            {
                str2.Copy(0, str1, 16, str2.Length);
            }

            var duration = DateTime.Now - startTime;
            Console.WriteLine($"total:{duration:c}");
        }

        [Test]
        public void SpecialCase()
        {
            var source = "the qqqqqquick brown fox quickly ate the rat".ToCharArray();
            var sought = "quick".ToCharArray();

            var position = source.SearchInCharArray(0, sought, 0);
            Assert.AreEqual(9, position);

            position = source.ReverseSearchInCharArray(sought);
            Assert.AreEqual(25, position);

            sought   = "qqquick".ToCharArray();
            position = source.SearchInCharArray(0, sought, 0);
            Assert.AreEqual(7, position);

            position = source.ReverseSearchInCharArray(sought);
            Assert.AreEqual(7, position);
        }

        [Test]
        [Explicit]
        public void PerformanceCompareStandard()
        {
            var str1StringValue = "the quick brown fox";
            var str2StringValue = "the quick brown fox quickly ate the rat";

            var startTime = DateTime.Now;

            for (var i = 0; i < 10000000; i++)
            {
                var valStr = string.Compare(str1StringValue, str2StringValue, StringComparison.Ordinal);
                if (valStr >= 0)
                    throw new InvalidOperationException("bugger");
            }

            var duration = DateTime.Now - startTime;
            Console.WriteLine($"total:{duration:c}");
        }

        [Test]
        [Explicit]
        public void PerformanceCompareCustom()
        {
            var str1 = "the quick brown fox".ToCharArray();
            var str2 = "the quick brown fox quickly ate the rat".ToCharArray();

            var startTime = DateTime.Now;

            for (var i = 0; i < 10000000; i++)
            {
                var val = str1.CompareCharArray(0, str2, 0, str2.Length);
                if (val >= 0)
                    throw new InvalidOperationException("bugger");
            }

            var duration = DateTime.Now - startTime;
            Console.WriteLine($"total:{duration:c}");
        }

        [Test]
        [Explicit]
        public void PerformanceIndexOfStandard()
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
        public void PerformanceIndexOfCustom()
        {
            var source = "the quick brown fox quickly ate the rat".ToCharArray();
            var sought = "quick".ToCharArray();

            var startTime = DateTime.Now;

            for (var i = 0; i < 10000000; i++)
            {
                var position = source.SearchInCharArray(5, sought, 0);
                if (position != 20)
                    throw new InvalidOperationException("bugger");
            }

            var duration = DateTime.Now - startTime;
            Console.WriteLine($"total:{duration:c}");
        }

        [Test]
        public void Compare()
        {
            var str1StringValue = "the quick brown fox";
            var str2StringValue = "the quick brown fox quickly ate the rat";
            var str3StringValue = "quick";
            var str4StringValue = "the quick brown fox";
            var str1            = str1StringValue.ToCharArray();
            var str2            = str2StringValue.ToCharArray();
            var str3            = str3StringValue.ToCharArray();
            var str4            = str4StringValue.ToCharArray();

            //shorter with longer
            var valStr = string.Compare(str1StringValue, str2StringValue, StringComparison.Ordinal).SignOnly();
            var val    = str1.CompareCharArray(0, str2, 0, str2.Length).SignOnly();
            Assert.AreEqual(valStr, val);

            //shorter with longer but with shorter length 
            val = str1.CompareCharArray(0, str2, 0, str1.Length).SignOnly();
            Assert.AreEqual(0, val);

            //longer with shorter
            valStr = string.Compare(str2StringValue, str1StringValue, StringComparison.Ordinal).SignOnly();
            val    = str2.CompareCharArray(0, str1, 0, str2.Length).SignOnly();
            Assert.AreEqual(valStr, val);

            //longer with shorter but with shorter length
            val = str2.CompareCharArray(0, str1, 0, str1.Length).SignOnly();
            Assert.AreEqual(0, val);

            //different
            valStr = string.Compare(str1StringValue, str3StringValue, StringComparison.Ordinal).SignOnly();
            val    = str1.CompareCharArray(0, str3, 0, str1.Length).SignOnly();
            Assert.AreEqual(valStr, val);

            //different
            valStr = string.Compare(str3StringValue, str1StringValue, StringComparison.Ordinal).SignOnly();
            val    = str3.CompareCharArray(0, str1, 0, str1.Length).SignOnly();
            Assert.AreEqual(valStr, val);

            val = str3.CompareCharArray(0, str1, 0, str3.Length).SignOnly();
            Assert.AreEqual(-1, val);

            //different but same substring with correct index and length
            val = str3.CompareCharArray(0, str1, 4, str3.Length).SignOnly();
            Assert.AreEqual(0, val);

            //different
            valStr = string.Compare(str2StringValue, str3StringValue, StringComparison.Ordinal).SignOnly();
            val    = str2.CompareCharArray(0, str3, 0, str1.Length).SignOnly();
            Assert.AreEqual(valStr, val);

            //different
            valStr = string.Compare(str3StringValue, str2StringValue, StringComparison.Ordinal).SignOnly();
            val    = str3.CompareCharArray(0, str2, 0, str1.Length).SignOnly();
            Assert.AreEqual(valStr, val);

            //same
            valStr = string.Compare(str1StringValue, str4StringValue, StringComparison.Ordinal).SignOnly();
            val    = str1.CompareCharArray(0, str4, 0, str1.Length).SignOnly();
            Assert.AreEqual(valStr, val);
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

        [Test]
        public void LockSlimTest()
        {
            ReaderWriterLockSlim lockObject = new(LockRecursionPolicy.SupportsRecursion);
            var                  startTime  = DateTime.Now;
            var                  source     = "the quick brown fox quickly ate the rat".ToCharArray();
            var                  sought     = "quick".ToCharArray();

            for (var i = 0; i < 10000000; i++)
            {
                try
                {
                    lockObject.EnterWriteLock();
                    var position = source.SearchInCharArray(5, sought, 0);
                    if (position != 20)
                        throw new InvalidOperationException("bugger");
                }
                finally
                {
                    lockObject.ExitWriteLock();
                }
            }

            var duration = DateTime.Now - startTime;
            Console.WriteLine($"total:{duration:c}");
        }

        [Test]
        public void LockTest()
        {
            ReaderWriterLock lockObject = new();
            var              startTime  = DateTime.Now;
            var              source     = "the quick brown fox quickly ate the rat".ToCharArray();
            var              sought     = "quick".ToCharArray();

            for (var i = 0; i < 10000000; i++)
            {
                try
                {
                    lockObject.AcquireWriterLock(1000);
                    var position = source.SearchInCharArray(5, sought, 0);
                    if (position != 20)
                        throw new InvalidOperationException("bugger");
                }
                finally
                {
                    lockObject.ReleaseWriterLock();
                }
            }

            var duration = DateTime.Now - startTime;
            Console.WriteLine($"total:{duration:c}");
        }

        [Test]
        public void DisposableLockTest()
        {
            ReaderWriterLockSlim lockObject = new(LockRecursionPolicy.SupportsRecursion);
            var                  startTime  = DateTime.Now;
            var                  source     = "the quick brown fox quickly ate the rat".ToCharArray();
            var                  sought     = "quick".ToCharArray();

            for (var i = 0; i < 10000000; i++)
            {
                using (lockObject.WriteLock())
                {
                    var position = source.SearchInCharArray(5, sought, 0);
                    if (position != 20)
                        throw new InvalidOperationException("bugger");
                }
            }

            var duration = DateTime.Now - startTime;
            Console.WriteLine($"total:{duration:c}");
        }
    }
}