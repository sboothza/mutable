//#define USE_DUMMY_LOCKER

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;

// ReSharper disable ForCanBeConvertedToForeach
// ReSharper disable LoopCanBeConvertedToQuery
// ReSharper disable UnusedMember.Global
// ReSharper disable SpecifyACultureInStringConversionExplicitly

namespace Mutable
{
    /// <summary>
    ///     Extensions to char[] and other helpers for <see cref="MutableString" /> <see cref="Locker" />
    /// </summary>
    public static class Extensions
    {
        public static char[] SafeClone(this char[] array)
        {
            if (array == null)
                return Array.Empty<char>();
            return array.Clone() as char[];
        }

        public static IEnumerable<MutableString> ToMutable(this    IEnumerable<string>        values) => values.Select(s => new MutableString(s));
        public static IEnumerable<MutableString> ToMutable(this    IEnumerable<char[]>        values) => values.Select(s => new MutableString(s));
        public static IEnumerable<string>        ToStrings(this    IEnumerable<MutableString> values) => values.Select(s => s.ToString());
        public static MutableString              ToMutable(this    string                     value)  => new(value);
        public static IEnumerable<char[]>        ToCharArrays(this IEnumerable<MutableString> values) => values.Select(v => (char[])v);

        public static int SearchInCharArray(this char[] source, char[] sought, int initial = 0)
        {
            // base case 1: sought is NULL or empty
            if (sought.Length == 0)
                return 0;

            // base case 2: source is NULL, or source's length is less than sought
            if (sought.Length > source.Length)
                return -1;

            for (int sourceIndex = initial, soughtIndex = 0; sourceIndex < source.Length; sourceIndex++)
                if (source[sourceIndex] == sought[soughtIndex])
                {
                    if (++soughtIndex == sought.Length)
                        return sourceIndex - soughtIndex + 1;
                }
                else if (soughtIndex > 0)
                {
                    soughtIndex = 0;
                    sourceIndex--; // since i will be incremented in the next iteration
                }

            return -1;
        }

        public static int ReverseSearchInCharArray(this char[] source, char[] sought, int initial = 0)
        {
            if (initial == 0)
                initial = source.Length - 1;

            // base case 1: sought is NULL or empty
            if (sought.Length == 0)
                return 0;

            // base case 2: source is NULL, or source's length is less than sought
            if (sought.Length > source.Length)
                return -1;

            for (int sourceIndex = initial, soughtIndex = sought.Length - 1; sourceIndex >= 0; sourceIndex--)
                if (source[sourceIndex] == sought[soughtIndex])
                {
                    if (--soughtIndex < 0)
                        return sourceIndex;
                }
                else if (soughtIndex < sought.Length - 1)
                {
                    soughtIndex = sought.Length - 1;
                    sourceIndex++; // since i will be decremented in the next iteration
                }

            return -1;
        }

        public static int CompareCharArray(this char[] source, char[] other)
        {
            if (source.Length == 0)
                return other.Length == 0 ? 0 : 1;

            var sourceIndex = 0;
            var otherIndex  = 0;
            while (sourceIndex < source.Length && otherIndex < other.Length)
            {
                // if characters differ, or end of the second string is reached
                if (source[sourceIndex] != other[otherIndex])
                    break;

                // move to the next pair of characters
                sourceIndex++;
                otherIndex++;
            }

            if (sourceIndex == source.Length)
                if (otherIndex == other.Length)
                    return 0;
                else
                    return -1;

            if (otherIndex == other.Length)
                return 1;

            return source[sourceIndex] - other[otherIndex];
        }

        public static void ToLower(this char[] source)
        {
            for (var i = 0; i < source.Length; i++)
                source[i] = char.ToLower(source[i]);
        }

        public static void ToUpper(this char[] source)
        {
            for (var i = 0; i < source.Length; i++)
                source[i] = char.ToUpper(source[i]);
        }

        public static int FirstNonNull<T>(this T[] source, int index = 0)
        {
            for (var i = index; i < source.Length; i++)
                if (source[i] is not null)
                    return i;
            return -1;
        }

        public static int FirstNull<T>(this T[] source, int index = 0)
        {
            for (var i = index; i < source.Length; i++)
                if (source[i] is null)
                    return i;
            return -1;
        }

        public static int LastNonNull<T>(this T[] source, int index = 0)
        {
            if (index == 0)
                index = source.Length;

            for (var i = index; i > 0; i--)
                if (source[i] is not null)
                    return i;
            return -1;
        }

        public static int LastNull<T>(this T[] source, int index = 0)
        {
            if (index == 0)
                index = source.Length;

            for (var i = index; i > 0; i--)
                if (source[i] is null)
                    return i;
            return -1;
        }

        public static IEnumerable<char[]> Split(this char[] source, char separator)
        {
            if (source == null || source.Length == 0)
                yield break;

            var index     = 0;
            var lastIndex = 0;
            while (index < source.Length)
            {
                if (source[index++] != separator)
                    continue;

                yield return source[lastIndex..(index - 1)];
                lastIndex = index;
            }

            yield return source[lastIndex..];
        }

        public static bool HasWhitespace(this char[] valueArray)
        {
            for (var i = 0; i < valueArray.Length; i++)
                if (char.IsWhiteSpace(valueArray[i]))
                    return true;
            return false;
        }

        public static bool HasNonWhitespace(this char[] valueArray)
        {
            for (var i = 0; i < valueArray.Length; i++)
                if (!char.IsWhiteSpace(valueArray[i]))
                    return true;
            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static IDisposable GetReadLock(this ReaderWriterLockSlim lockObject)
        {
#if USE_DUMMY_LOCKER
            return new DummyLocker();
#else
            return new Locker(lockObject, Locker.LockType.Read);
#endif
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static IDisposable GetWriteLock(this ReaderWriterLockSlim lockObject)
        {
#if USE_DUMMY_LOCKER
            return new DummyLocker();
#else
            return new Locker(lockObject, Locker.LockType.Write);
#endif
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static IDisposable GetUpgradeableLock(this ReaderWriterLockSlim lockObject)
        {
#if USE_DUMMY_LOCKER
            return new DummyLocker();
#else
            return new Locker(lockObject, Locker.LockType.Upgradeable);
#endif
        }
    }
}