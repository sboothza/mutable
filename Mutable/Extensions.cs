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

        public static int SearchInCharArray(this char[] source, int sourceInd, char[] sought, int soughtInd = 0)
        {
            var sourceLength = source.Length;
            var soughtLength = sought.Length;

            // base case 1: sought is NULL or empty
            if (soughtLength == 0)
                return 0;

            // base case 2: source is NULL, or source's length is less than sought
            if (soughtLength - soughtInd > sourceLength - sourceInd)
                return -1;

            for (int sourceIndex = sourceInd, soughtIndex = soughtInd; sourceIndex < sourceLength; sourceIndex++)
                if (source[sourceIndex] == sought[soughtIndex])
                {
                    if (++soughtIndex == soughtLength)
                        return sourceIndex - soughtIndex + 1;
                }
                else if (soughtIndex > 0)
                {
                    soughtIndex = soughtInd;
                    sourceIndex--; // since i will be incremented in the next iteration
                }

            return -1;
        }

        public static int SignOnly(this int value) => value == 0 ? 0 : value / Math.Abs(value);

        public static IEnumerable<T> EnumerateWith<T>(this IEnumerable<T> list, T item)
        {
            yield return item;
            using (var e = list.GetEnumerator())
            {
                while (e.MoveNext())
                    yield return e.Current;
            }
        }

        public static int Min(this int value, params int[] values) => Math.Min(value, values.Min());

        public static unsafe int CompareCharArray(this char[] source, int sourceIndex, char[] target, int targetIndex, int length)
        {
            var matchLength  = sizeof(char) * length;
            var sourceLength = Math.Min(matchLength, sizeof(char) * (source.Length - sourceIndex));
            var targetLength = Math.Min(matchLength, sizeof(char) * (target.Length - targetIndex));

            var remaining = Math.Min(sourceLength, targetLength);

            fixed (char* sourcePtr = &source[sourceIndex], targetPtr = &target[targetIndex])
            {
                var sourceBytePtr = (byte*)sourcePtr;
                var targetBytePtr = (byte*)targetPtr;

                //compare as ulong
                var num = remaining >> 3;
                for (var i = 0; i < num; i++)
                {
                    var sourceValue = *(ulong*)sourceBytePtr;
                    var targetValue = *(ulong*)targetBytePtr;
                    if (sourceValue != targetValue)
                        return (int)(targetValue - sourceValue);

                    sourceBytePtr += 8;
                    targetBytePtr += 8;
                }

                //compare shorts
                num = (remaining & 7) >> 1;
                for (var i = 0; i < num; i++)
                {
                    var sourceValue = *(ushort*)sourceBytePtr;
                    var targetValue = *(ushort*)targetBytePtr;
                    if (sourceValue != targetValue)
                        return targetValue - sourceValue;

                    sourceBytePtr += 2;
                    targetBytePtr += 2;
                }

                return sourceLength - targetLength;
            }
        }

        /// <summary>
        ///     Copy char array
        ///     Same speed as Array.Copy so unused
        /// </summary>
        /// <param name="source"></param>
        /// <param name="sourceIndex"></param>
        /// <param name="target"></param>
        /// <param name="targetIndex"></param>
        /// <param name="length"></param>
        /// <exception cref="IndexOutOfRangeException"></exception>
        public static unsafe void Copy(this char[] source, int sourceIndex, char[] target, int targetIndex, int length)
        {
            if (sourceIndex + length > source.Length)
                throw new IndexOutOfRangeException("Source is out of bounds");
            if (targetIndex + length > target.Length)
                throw new IndexOutOfRangeException("Target is out of bounds");

            var lengthBytes = sizeof(char) * length;

            fixed (char* sourcePtr = &source[sourceIndex], targetPtr = &target[targetIndex])
            {
                var sourceBytePtr = (byte*)sourcePtr;
                var targetBytePtr = (byte*)targetPtr;

                //copy as ulong
                if (lengthBytes >= 8)
                {
                    var sourceLongPtr = (ulong*)sourcePtr;
                    var targetLongPtr = (ulong*)targetPtr;

                    var batchCounter = lengthBytes >> 3;

                    do
                    {
                        *targetLongPtr++ = *sourceLongPtr++;
                    } while (--batchCounter > 0);

                    sourceLongPtr--;
                    targetLongPtr--;

                    lengthBytes   &= 7;
                    sourceBytePtr =  (byte*)sourceLongPtr;
                    sourceBytePtr++;
                    targetBytePtr = (byte*)targetLongPtr;
                    targetBytePtr++;
                }

                //copy remainder as byte
                while (lengthBytes > 0)
                {
                    *targetBytePtr++ = *sourceBytePtr++;
                    lengthBytes--;
                }
            }
        }

        /// <summary>
        ///     Experimental - not used
        /// </summary>
        /// <param name="text"></param>
        /// <param name="pattern"></param>
        /// <returns></returns>
        public static int search(this char[] text, char[] pattern)
        {
            const int B = 131;
            int       j;
            var       found  = false;
            var       result = 0;

            int m;
            if (pattern.Length == 0)
                return 0;

            var Bm         = 1;
            var hpat       = 0;
            var htext      = 0;
            var textLength = text.Length;

            for (m = 0; m < textLength && m < pattern.Length; m++)
            {
                Bm    *= B;
                hpat  =  hpat  * B + pattern[m];
                htext =  htext * B + text[m];
            }

            if (m >= textLength && m <= pattern.Length)
                return -1;

            for (j = m;; j++)
            {
                if (hpat == htext && CompareCharArray(text, j - m, pattern, 0, m) == 0)
                    return j - m;
                if (j >= textLength)
                    return -1;
                htext = htext * B - text[j - m] * Bm + text[j];
            }
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

        public static int CompareCharArrayOld(this char[] source, char[] other)
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

        
    }
}