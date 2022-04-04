using System;
using System.Threading;
using Mutable;
using NUnit.Framework;

namespace MutableTests
{
    [TestFixture]
    public class LockTests
    {
        [Test]
        public void LockSlimTest()
        {
            ReaderWriterLockSlim lockObject = new(LockRecursionPolicy.SupportsRecursion);
            var                  startTime  = DateTime.Now;
            var                  source     = "the quick brown fox quickly ate the rat".ToCharArray();
            var                  sought     = "quick".ToCharArray();

            for (var i = 0; i < 10000000; i++)
                try
                {
                    lockObject.EnterWriteLock();
                    var position = source.SearchInCharArray(5, sought);
                    if (position != 20)
                        throw new InvalidOperationException("bugger");
                }
                finally
                {
                    lockObject.ExitWriteLock();
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
                try
                {
                    lockObject.AcquireWriterLock(1000);
                    var position = source.SearchInCharArray(5, sought);
                    if (position != 20)
                        throw new InvalidOperationException("bugger");
                }
                finally
                {
                    lockObject.ReleaseWriterLock();
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
                using (lockObject.WriteLock())
                {
                    var position = source.SearchInCharArray(5, sought);
                    if (position != 20)
                        throw new InvalidOperationException("bugger");
                }

            var duration = DateTime.Now - startTime;
            Console.WriteLine($"total:{duration:c}");
        }
    }
}