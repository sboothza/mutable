using System;
using System.Runtime.CompilerServices;
using System.Threading;

namespace Mutable
{
    public static class LockExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static IDisposable ReadLock(this ReaderWriterLockSlim lockObject) => new Locker(lockObject, Locker.LockType.Read);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static IDisposable WriteLock(this ReaderWriterLockSlim lockObject) => new Locker(lockObject, Locker.LockType.Write);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static IDisposable UpgradeableLock(this ReaderWriterLockSlim lockObject) => new Locker(lockObject, Locker.LockType.Upgradeable);

        // [MethodImpl(MethodImplOptions.AggressiveInlining)]
        // public static void GetReadLock(this ReaderWriterLockSlim lockObject) => Lock(lockObject, Locker.LockType.Read);
        //
        // [MethodImpl(MethodImplOptions.AggressiveInlining)]
        // public static void GetWriteLock(this ReaderWriterLockSlim lockObject) => Lock(lockObject, Locker.LockType.Write);
        //
        // [MethodImpl(MethodImplOptions.AggressiveInlining)]
        // public static void GetUpgradeableLock(this ReaderWriterLockSlim lockObject) => Lock(lockObject, Locker.LockType.Upgradeable);
        //
        // [MethodImpl(MethodImplOptions.AggressiveInlining)]
        // public static void UnlockReadLock(this ReaderWriterLockSlim lockObject) => Unlock(lockObject, Locker.LockType.Read);
        //
        // [MethodImpl(MethodImplOptions.AggressiveInlining)]
        // public static void UnlockWriteLock(this ReaderWriterLockSlim lockObject) => Unlock(lockObject, Locker.LockType.Write);
        //
        // [MethodImpl(MethodImplOptions.AggressiveInlining)]
        // public static void UnlockUpgradeableLock(this ReaderWriterLockSlim lockObject) => Unlock(lockObject, Locker.LockType.Upgradeable);

        // [MethodImpl(MethodImplOptions.AggressiveInlining)]
        // private static void Lock(this ReaderWriterLockSlim lockObject, Locker.LockType lockType)
        // {
        //     switch (lockType)
        //     {
        //         case Locker.LockType.Read:
        //             lockObject.EnterReadLock();
        //             break;
        //
        //         case Locker.LockType.Write:
        //             lockObject.EnterWriteLock();
        //             break;
        //
        //         case Locker.LockType.Upgradeable:
        //             lockObject.EnterUpgradeableReadLock();
        //             break;
        //
        //         default:
        //             throw new ArgumentOutOfRangeException(nameof(lockType), lockType, null);
        //     }
        // }
        //
        // [MethodImpl(MethodImplOptions.AggressiveInlining)]
        // private static void Unlock(this ReaderWriterLockSlim lockObject, Locker.LockType lockType)
        // {
        //     switch (lockType)
        //     {
        //         case Locker.LockType.Read:
        //             lockObject.ExitReadLock();
        //             break;
        //
        //         case Locker.LockType.Write:
        //             lockObject.ExitWriteLock();
        //             break;
        //
        //         case Locker.LockType.Upgradeable:
        //             lockObject.ExitUpgradeableReadLock();
        //             break;
        //     }
        // }
    }
}