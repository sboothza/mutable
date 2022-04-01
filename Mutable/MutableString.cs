#nullable enable
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;

// ReSharper disable UnusedMethodReturnValue.Global
// ReSharper disable SpecifyACultureInStringConversionExplicitly

// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedMember.Global

namespace Mutable
{
    /// <summary>
    ///     Mutable string class
    ///     Exposes the same interface as <see cref="System.String" /> as far as possible
    ///     Allows internal changes to the string without creating new instance
    ///     For faster manipulation
    /// </summary>
    public class MutableString : IEnumerable<char>, IComparable, IComparable<string>, IComparable<MutableString>, IConvertible, IEquatable<string>, IEquatable<MutableString>, ICloneable
    {
        private readonly ReaderWriterLockSlim _lock = new(LockRecursionPolicy.SupportsRecursion);
        private          char[]               _data;
        private          bool                 _dirty       = true;
        private          string               _stringValue = "";

#pragma warning disable CS8618
        public MutableString(int size) => Resize(size, false);
#pragma warning restore CS8618

        /// <summary>
        ///     Creates a new string with a copy of the contents of the array
        /// </summary>
        /// <param name="valueArray"></param>
        public MutableString(char[] valueArray) => _data = valueArray.SafeClone();

        /// <summary>
        ///     Creates a new string with a copy of the provided string
        /// </summary>
        /// <param name="value"></param>
        public MutableString(string value) => _data = value.ToCharArray().SafeClone();

        public int Length => _data.Length;

        public char this[int index]
        {
            get
            {
                using (_lock.GetReadLock())
                {
                    return index >= Length || index < 0 ? throw new IndexOutOfRangeException() : _data[index];
                }
            }
            set
            {
                using (_lock.GetWriteLock())
                {
                    if (index < 0 || _data.Length <= index)
                        return;

                    _data[index] = value;
                    _dirty       = true;
                }
            }
        }

        /// <summary>
        ///     Creates a copy of the string
        /// </summary>
        /// <returns></returns>
        public object Clone() => new MutableString(ToString());

        public int CompareTo(object? obj)
        {
            if (obj is MutableString mutableString)
                return Compare(this, mutableString);

            return 1;
        }

        public int               CompareTo(MutableString? value)                                        => Compare(this, value);
        public int               CompareTo(string?        value)                                        => value == null ? 1 : CompareInternal(value.ToCharArray());
        public TypeCode          GetTypeCode()                                                          => TypeCode.String;
        public bool              ToBoolean(IFormatProvider?  provider)                                  => Convert.ToBoolean(ToString(), provider);
        public byte              ToByte(IFormatProvider?     provider)                                  => Convert.ToByte(ToString(), provider);
        public char              ToChar(IFormatProvider?     provider)                                  => Convert.ToChar(ToString(), provider);
        public DateTime          ToDateTime(IFormatProvider? provider)                                  => Convert.ToDateTime(ToString(), provider);
        public decimal           ToDecimal(IFormatProvider?  provider)                                  => Convert.ToDecimal(ToString(), provider);
        public double            ToDouble(IFormatProvider?   provider)                                  => Convert.ToDouble(ToString(), provider);
        public short             ToInt16(IFormatProvider?    provider)                                  => Convert.ToInt16(ToString(), provider);
        public int               ToInt32(IFormatProvider?    provider)                                  => Convert.ToInt32(ToString(), provider);
        public long              ToInt64(IFormatProvider?    provider)                                  => Convert.ToInt64(ToString(), provider);
        public sbyte             ToSByte(IFormatProvider?    provider)                                  => Convert.ToSByte(ToString(), provider);
        public float             ToSingle(IFormatProvider?   provider)                                  => Convert.ToSingle(ToString(), provider);
        public string            ToString(IFormatProvider?   provider)                                  => ToString();
        public object            ToType(Type                 conversionType, IFormatProvider? provider) => Convert.ChangeType(ToString(), conversionType, provider);
        public ushort            ToUInt16(IFormatProvider?   provider) => Convert.ToUInt16(ToString(), provider);
        public uint              ToUInt32(IFormatProvider?   provider) => Convert.ToUInt32(ToString(), provider);
        public ulong             ToUInt64(IFormatProvider?   provider) => Convert.ToUInt64(ToString(), provider);
        IEnumerator IEnumerable. GetEnumerator()                       => GetEnumerator();
        public IEnumerator<char> GetEnumerator()                       => new ArrayEnumerator<char>(_data);
        public bool              Equals(MutableString? obj)            => Equals(this, obj);

        public bool Equals(string? value)
        {
            if (value == null)
                return false;

            if (Length != value.Length)
                return false;

            return CompareInternal(value.ToCharArray()) == 0;
        }

        public override string ToString()
        {
            using (_lock.GetReadLock())
            {
                // ReSharper disable once InvertIf
                if (_dirty)
                {
                    _dirty       = false;
                    _stringValue = new string(_data);
                }

                return _stringValue;
            }
        }

        private void Resize(int size, bool moveExisting, int startIndex = 0)
        {
            using (_lock.GetWriteLock())
            {
                var newBuffer = new char[size];
                if (moveExisting)
                    Array.Copy(_data, 0, newBuffer, startIndex, _data.Length);
                _data = newBuffer;
            }
        }

        public string GetSubString(int index, int length = 0) => new(GetSlice(index, length));

        public char[] GetSlice(int index, int length = 0)
        {
            using (_lock.GetReadLock())
            {
                if (length == 0)
                    length = _data.Length - index;

                return _data[index..(index + length)];
            }
        }

        /// <summary>
        ///     Overwrites a piece of the string with the provided value
        /// </summary>
        /// <param name="index"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public MutableString SetSubString(int index, string value) => SetSubString(index, value.ToCharArray());

        /// <summary>
        ///     Overwrites a piece of the string with the provided value
        /// </summary>
        /// <param name="index"></param>
        /// <param name="valueArray"></param>
        /// <returns></returns>
        /// <exception cref="IndexOutOfRangeException"></exception>
        public MutableString SetSubString(int index, char[] valueArray)
        {
            using (_lock.GetWriteLock())
            {
                if (index + valueArray.Length >= _data.Length)
                    throw new IndexOutOfRangeException();

                Array.Copy(valueArray, 0, _data, index, valueArray.Length);
                _dirty = true;
            }

            return this;
        }

        /// <summary>
        ///     Fills a piece of the string with the provided char
        /// </summary>
        /// <param name="index"></param>
        /// <param name="length"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        /// <exception cref="IndexOutOfRangeException"></exception>
        public MutableString FillSubString(int index, int length, char value)
        {
            using (_lock.GetWriteLock())
            {
                if (index + length > _data.Length)
                    throw new IndexOutOfRangeException();

                Array.Fill(_data, value, index, length);
                _dirty = true;
            }

            return this;
        }

        /// <summary>
        ///     Resizes the string, inserting a space in the middle
        /// </summary>
        /// <param name="index"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        public MutableString Stretch(int index, int length)
        {
            using (_lock.GetWriteLock())
            {
                var newBuffer = new char[Length + length];
                Array.Copy(_data, 0, newBuffer, 0, index);
                Array.Copy(_data, index, newBuffer, index + length, Length - index);
                _data  = newBuffer;
                _dirty = true;
            }

            return this;
        }

        /// <summary>
        ///     Adds the provided value to the end
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public MutableString Append(MutableString value) => Append((char[])value);

        /// <summary>
        ///     Adds the provided value to the end
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public MutableString Append(string value) => Append(value.ToCharArray());

        /// <summary>
        ///     Adds the provided value to the end
        /// </summary>
        /// <param name="valueArray"></param>
        /// <returns></returns>
        public MutableString Append(char[] valueArray)
        {
            using (_lock.GetWriteLock())
            {
                var endOfArray = Length;
                Resize(Length + valueArray.Length, true);
                Array.Copy(valueArray, 0, _data, endOfArray, valueArray.Length);
                _dirty = true;
            }

            return this;
        }

        /// <summary>
        ///     Adds the provided value to the beginning
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public MutableString Prepend(MutableString value) => Prepend((char[])value);

        /// <summary>
        ///     Adds the provided value to the beginning
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public MutableString Prepend(string value) => Prepend(value.ToCharArray());

        /// <summary>
        ///     Adds the provided value to the beginning
        /// </summary>
        /// <param name="valueArray"></param>
        /// <returns></returns>
        public MutableString Prepend(char[] valueArray)
        {
            using (_lock.GetWriteLock())
            {
                Resize(Length + valueArray.Length, true, valueArray.Length);
                Array.Copy(valueArray, 0, _data, 0, valueArray.Length);
                _dirty = true;
            }

            return this;
        }

        /// <summary>
        ///     Inserts the value into the middle of the string, resizing it accordingly
        /// </summary>
        /// <param name="index"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public MutableString Insert(int index, MutableString value) => Insert(index, (char[])value);

        /// <summary>
        ///     Inserts the value into the middle of the string, resizing it accordingly
        /// </summary>
        /// <param name="index"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public MutableString Insert(int index, string value) => Insert(index, value.ToCharArray());

        /// <summary>
        ///     Inserts the value into the middle of the string, resizing it accordingly
        /// </summary>
        /// <param name="index"></param>
        /// <param name="valueArray"></param>
        /// <returns></returns>
        public MutableString Insert(int index, char[] valueArray)
        {
            using (_lock.GetWriteLock())
            {
                Stretch(index, valueArray.Length);
                Array.Copy(valueArray, 0, _data, index, valueArray.Length);
                _dirty = true;
            }

            return this;
        }

        /// <summary>
        ///     Finds the first index of the value from the provided index, in order
        /// </summary>
        /// <param name="value"></param>
        /// <param name="index"></param>
        /// <returns></returns>
        public int IndexOf(string value, int index = 0) => IndexOf(value.ToCharArray(), index);

        /// <summary>
        ///     Finds the first index of the value from the provided index, in order
        /// </summary>
        /// <param name="valueArray"></param>
        /// <param name="index"></param>
        /// <returns></returns>
        public int IndexOf(char[] valueArray, int index = 0)
        {
            using (_lock.GetReadLock())
            {
                if (index < 0 || index + valueArray.Length >= Length)
                    return -1;

                return _data.SearchInCharArray(valueArray, index);
            }
        }

        /// <summary>
        ///     Finds the first index of the value from the provided index, in order
        /// </summary>
        /// <param name="value"></param>
        /// <param name="index"></param>
        /// <returns></returns>
        public int IndexOf(MutableString value, int index = 0) => IndexOf((char[])value, index);

        /// <summary>
        ///     Checks for any whitespace chars in the string
        /// </summary>
        /// <returns></returns>
        public bool HasWhitespace() => _data.HasWhitespace();

        /// <summary>
        ///     Checks for any non-whitespace chars in the string
        /// </summary>
        /// <returns></returns>
        public bool HasNonWhitespace() => _data.HasNonWhitespace();

        /// <summary>
        ///     Finds the first index of the value from the provided index, in reverse order
        /// </summary>
        /// <param name="value"></param>
        /// <param name="index"></param>
        /// <returns></returns>
        public int LastIndexOf(string value, int index = 0) => LastIndexOf(value.ToCharArray(), index);

        /// <summary>
        ///     Finds the first index of the value from the provided index, in reverse order
        /// </summary>
        /// <param name="valueArray"></param>
        /// <param name="index"></param>
        /// <returns></returns>
        public int LastIndexOf(char[] valueArray, int index = 0)
        {
            using (_lock.GetReadLock())
            {
                if (index < 0 || index + valueArray.Length >= Length)
                    return -1;

                return _data.ReverseSearchInCharArray(valueArray, index);
            }
        }

        /// <summary>
        ///     Finds the first index of the value from the provided index, in reverse order
        /// </summary>
        /// <param name="value"></param>
        /// <param name="index"></param>
        /// <returns></returns>
        public int LastIndexOf(MutableString value, int index = 0) => LastIndexOf((char[])value, index);

        /// <summary>
        ///     Replaces all instances of 'find' with 'replace'
        ///     Resizes the string as necessary
        /// </summary>
        /// <param name="find"></param>
        /// <param name="replace"></param>
        /// <returns></returns>
        public MutableString Replace(MutableString find, MutableString replace) => Replace((char[])find, (char[])replace);

        /// <summary>
        ///     Replaces all instances of 'find' with 'replace'
        ///     Resizes the string as necessary
        /// </summary>
        /// <param name="findArray"></param>
        /// <param name="replaceArray"></param>
        /// <returns></returns>
        public MutableString Replace(char[] findArray, char[] replaceArray)
        {
            using (_lock.GetWriteLock())
            {
                var index = IndexOf(findArray);
                while (index >= 0)
                {
                    ReplaceOne(index, findArray.Length, replaceArray);
                    index = IndexOf(findArray);
                }
            }

            return this;
        }

        /// <summary>
        ///     Replaces all instances of 'find' with 'replace'
        ///     Resizes the string as necessary
        /// </summary>
        /// <param name="find"></param>
        /// <param name="replace"></param>
        /// <returns></returns>
        public MutableString Replace(string find, string replace) => Replace(find.ToCharArray(), replace.ToCharArray());

        /// <summary>
        ///     Replaces a single occurence of the sought value with the replacement value
        /// </summary>
        /// <param name="start"></param>
        /// <param name="length"></param>
        /// <param name="replaceArray"></param>
        private void ReplaceOne(int start, int length, char[] replaceArray)
        {
            var buffer = new char[Length - length + replaceArray.Length];
            Array.Copy(_data, 0, buffer, 0, start);
            Array.Copy(replaceArray, 0, buffer, start, replaceArray.Length);
            Array.Copy(_data, start + length, buffer, start + replaceArray.Length, Length - start - length);
            _data  = buffer;
            _dirty = true;
        }

        public override int GetHashCode() => ToString().GetHashCode();

        public override bool Equals(object? obj)
        {
            if (obj is null)
                return false;

            if (ReferenceEquals(this, obj))
                return true;

            return obj is MutableString str && Equals(this, str);
        }

        public static bool Equals(MutableString? a, MutableString? b)
        {
            if (a is null || b is null)
                return false;

            if (ReferenceEquals(a, b))
                return true;

            if (a.Length != b.Length)
                return false;

            return a.CompareInternal((char[])b) == 0;
        }

        protected int CompareInternal(char[] other)
        {
            using (_lock.GetReadLock())
            {
                return _data.CompareCharArray(other);
            }
        }

        public static int Compare(MutableString? a, MutableString? b)
        {
            if (a is null)
                return -1;

            if (b is null)
                return 1;

            return ReferenceEquals(a, b) ? 0 : a.CompareInternal((char[])b);
        }

        /// <summary>
        ///     Checks if the value exists within the string
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public bool Contains(MutableString value) => Contains((char[])value);

        /// <summary>
        ///     Checks if the value exists within the string
        /// </summary>
        /// <param name="valueArray"></param>
        /// <returns></returns>
        public bool Contains(char[] valueArray) => IndexOf(valueArray) >= 0;

        /// <summary>
        ///     Checks if the value exists within the string
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public bool Contains(string value) => Contains(value.ToCharArray());

        public static bool operator ==(MutableString a, MutableString b) => Equals(a, b);
        public static bool operator !=(MutableString a, MutableString b) => !Equals(a, b);
        public static bool operator <(MutableString  a, MutableString b) => Compare(a, b) < 0;
        public static bool operator >(MutableString  a, MutableString b) => Compare(a, b) > 0;
        public static bool operator <=(MutableString a, MutableString b) => Compare(a, b) <= 0;
        public static bool operator >=(MutableString a, MutableString b) => Compare(a, b) >= 0;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator string(MutableString value) => value.ToString();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static explicit operator char[](MutableString value) => value._data;

        public static MutableString operator +(MutableString a, MutableString b) => new MutableString(a).Append(b);

        /// <summary>
        ///     Changes the contents string to lowercase
        ///     Returns the same instance, not a new one
        /// </summary>
        /// <returns></returns>
        public MutableString Lower()
        {
            _data.ToLower();
            return this;
        }

        /// <summary>
        ///     Changes the contents string to uppercase
        ///     Returns the same instance, not a new one
        /// </summary>
        /// <returns></returns>
        public MutableString Upper()
        {
            _data.ToUpper();
            return this;
        }

        /// <summary>
        ///     Returns a new string that has been converted to lowercase
        ///     Similar behaviour as <see cref="System.String" />
        /// </summary>
        /// <returns></returns>
        public MutableString ToLower() => ToString().ToMutable().Lower();

        /// Returns a new string that has been converted to uppercase
        /// Similar behaviour as
        /// <see cref="System.String" />
        public MutableString ToUpper() => ToString().ToMutable().Upper();

        /// <summary>
        ///     Iterator returning the string broken up by the separator char
        /// </summary>
        /// <param name="separator"></param>
        /// <returns></returns>
        public IEnumerable<MutableString> Split(char separator)
        {
            using (_lock.GetReadLock())
            {
                return _data.Split(separator).ToMutable();
            }
        }

        private MutableString Trim(bool head, bool tail)
        {
            using (_lock.GetWriteLock())
            {
                var end   = Length - 1;
                var start = 0;

                if (head)
                    for (start = 0; start < Length; start++)
                        if (!char.IsWhiteSpace(this[start]))
                            break;

                if (tail)
                    for (end = Length - 1; end >= start; end--)
                        if (!char.IsWhiteSpace(this[end]))
                            break;

                var len    = end - start + 1;
                var buffer = new char[len];
                Array.Copy(_data, start, buffer, 0, len);
                _data  = buffer;
                _dirty = true;
            }

            return this;
        }

        /// <summary>
        ///     Removes leading and trailing whitespace from the string
        ///     Returns the same instance, not a new one
        /// </summary>
        /// <returns></returns>
        public MutableString Trim() => Trim(true, true);

        /// <summary>
        ///     Removes leading whitespace from the string
        ///     Returns the same instance, not a new one
        /// </summary>
        /// <returns></returns>
        public MutableString TrimStart() => Trim(true, false);

        /// <summary>
        ///     Removes trailing whitespace from the string
        ///     Returns the same instance, not a new one
        /// </summary>
        /// <returns></returns>
        public MutableString TrimEnd() => Trim(false, true);
    }
}