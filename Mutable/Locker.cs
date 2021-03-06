using System;
using System.Threading;

// ReSharper disable ClassWithVirtualMembersNeverInherited.Global

namespace Mutable
{
    /// <summary>
    /// Disposable locking object
    /// Allows for simple lock constructs like:
    /// using (lockObject.WriteLock())
    /// {...}
    /// Not very fast - the overhead from creating and disposing makes it unsuitable for performance code
    /// But it's very easy to use and will be fine in most conditions
    /// </summary>
    public class Locker : IDisposable
    {
        public enum LockType
        {
            Read,
            Write,
            Upgradeable
        }

        private readonly ReaderWriterLockSlim _lock;
        private readonly LockType             _lockType;

        public Locker(ReaderWriterLockSlim lockObject, LockType lockType)
        {
            _lock     = lockObject;
            _lockType = lockType;

            switch (lockType)
            {
                case LockType.Read:
                    _lock.EnterReadLock();
                    break;

                case LockType.Write:
                    _lock.EnterWriteLock();
                    break;

                case LockType.Upgradeable:
                    _lock.EnterUpgradeableReadLock();
                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(lockType), lockType, null);
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposing)
                return;

            // ReSharper disable once SwitchStatementHandlesSomeKnownEnumValuesWithDefault
            switch (_lockType)
            {
                case LockType.Read:
                    _lock.ExitReadLock();
                    break;

                case LockType.Write:
                    _lock.ExitWriteLock();
                    break;

                case LockType.Upgradeable:
                    _lock.ExitUpgradeableReadLock();
                    break;
            }
        }
    }
}