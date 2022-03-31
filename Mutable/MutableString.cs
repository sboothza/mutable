#nullable enable
using System;
using System.Collections;
using System.Collections.Generic;

namespace Mutable
{
    public class MutableString : IEnumerable<char>, IComparable, IComparable<string>, IComparable<MutableString>, IConvertible, IEquatable<string>, IEquatable<MutableString>, ICloneable
    {
        private          char[] _data;
        public           int    Length => _data.Length;
        private          bool   _dirty       = true;
        private          string _stringValue = "";
        private readonly object _lock        = new();

        public override string ToString()
        {
            lock (_lock)
            {
                if (_dirty)
                {
                    _dirty       = false;
                    _stringValue = new string(_data);
                }

                return _stringValue;
            }
        }

        public object Clone()
        {
            return new MutableString(ToString());
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        private void Resize(int size, bool moveExisting, int startIndex = 0)
        {
            lock (_lock)
            {
                var newBuffer = new char[size];
                if (moveExisting)
                    Array.Copy(_data, 0, newBuffer, startIndex, _data.Length);
                _data = newBuffer;
            }
        }

        public MutableString(int size)
        {
            Resize(size, false);
        }

        public MutableString(char[] valueArray)
        {
            _data = valueArray.Clone() as char[];
        }

        public MutableString(string value)
        {
            _data = value.ToCharArray().Clone() as char[];
        }

        public char this[int index]
        {
            get => index >= Length || index < 0 ? throw new IndexOutOfRangeException() : _data[index];
            set
            {
                if (index < 0 || _data.Length <= index)
                    return;
                _data[index] = value;
                _dirty       = true;
            }
        }

        public string GetSubString(int index, int length = 0) => new string(GetSlice(index, length));

        public char[] GetSlice(int index, int length = 0)
        {
            if (length == 0)
                length = _data.Length - index;
            return _data[index..(index + length)];
        }

        public MutableString SetSubString(int index, string value) => SetSubString(index, value.ToCharArray());

        public MutableString SetSubString(int index, char[] valueArray)
        {
            lock (_lock)
            {
                if (index + valueArray.Length >= _data.Length)
                    throw new IndexOutOfRangeException();

                Array.Copy(valueArray, 0, _data, index, valueArray.Length);
                _dirty = true;
            }

            return this;
        }

        public MutableString FillSubString(int index, int length, char value)
        {
            lock (_lock)
            {
                if (index + length > _data.Length)
                    throw new IndexOutOfRangeException();

                Array.Fill(_data, value, index, length);
                _dirty = true;
            }

            return this;
        }

        public MutableString Stretch(int index, int length)
        {
            lock (_lock)
            {
                var newBuffer = new char[Length + length];
                Array.Copy(_data, 0, newBuffer, 0, index);
                Array.Copy(_data, index, newBuffer, index + length, Length - index);
                _data  = newBuffer;
                _dirty = true;
            }

            return this;
        }

        public MutableString Append(MutableString value) => Append((char[])value);
        public MutableString Append(string        value) => Append(value.ToCharArray());

        public MutableString Append(char[] valueArray)
        {
            lock (_lock)
            {
                var endOfArray = Length;
                Resize(Length + valueArray.Length, true);
                Array.Copy(valueArray, 0, _data, endOfArray, valueArray.Length);
                _dirty = true;
            }

            return this;
        }

        public MutableString Prepend(MutableString value) => Prepend((char[])value);
        public MutableString Prepend(string        value) => Prepend(value.ToCharArray());

        public MutableString Prepend(char[] valueArray)
        {
            lock (_lock)
            {
                Resize(Length + valueArray.Length, true, valueArray.Length);
                Array.Copy(valueArray, 0, _data, 0, valueArray.Length);
                _dirty = true;
            }

            return this;
        }

        public MutableString Insert(int index, MutableString value) => Insert(index, (char[])value);
        public MutableString Insert(int index, string        value) => Insert(index, value.ToCharArray());

        public MutableString Insert(int index, char[] valueArray)
        {
            lock (_lock)
            {
                Stretch(index, valueArray.Length);
                Array.Copy(valueArray, 0, _data, index, valueArray.Length);
                _dirty = true;
            }

            return this;
        }

        public int IndexOf(string value, int index = 0) => IndexOf(value.ToCharArray(), index);

        public int IndexOf(char[] valueArray, int index = 0)
        {
            if (index < 0 || index + valueArray.Length >= Length)
                return -1;

            return _data.SearchInCharArray(valueArray, index);
        }

        public int IndexOf(MutableString value, int index = 0) => IndexOf((char[])value, index);

        public bool HasWhitespace()
        {
            for (var i = 0; i < Length; i++)
                if (char.IsWhiteSpace(_data[i]))
                    return true;
            return false;
        }

        public bool HasNonWhitespace()
        {
            for (var i = 0; i < Length; i++)
                if (!char.IsWhiteSpace(_data[i]))
                    return true;
            return false;
        }

        public int LastIndexOf(string value, int index = 0) => LastIndexOf(value.ToCharArray(), index);

        public int LastIndexOf(char[] valueArray, int index = 0)
        {
            if (index < 0 || index + valueArray.Length >= Length)
                return -1;

            return _data.ReverseSearchInCharArray(valueArray, index);
        }

        public int LastIndexOf(MutableString value, int index = 0) => LastIndexOf((char[])value, index);
        public MutableString Replace(MutableString find, MutableString replace) => Replace((char[])find, (char[])replace);
        public MutableString Replace(string find, string replace) => Replace(find.ToCharArray(), replace.ToCharArray());

        private void ReplaceOne(int start, int length, char[] replaceArray)
        {
            var buffer = new char[Length - length + replaceArray.Length];
            Array.Copy(_data, 0, buffer, 0, start);
            Array.Copy(replaceArray, 0, buffer, start, replaceArray.Length);
            Array.Copy(_data, start + length, buffer, start + replaceArray.Length, Length - start - length);
            _data  = buffer;
            _dirty = true;
        }

        public MutableString Replace(char[] findArray, char[] replaceArray)
        {
            lock (_lock)
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

        public override int GetHashCode() => ToString().GetHashCode();
        public IEnumerator<char> GetEnumerator() => new ArrayEnumerator<char>(_data);

        public bool Equals(string? value)
        {
            if (value == null)
                return false;

            if (Length != value.Length)
                return false;

            return _data.CompareCharArray(value.ToCharArray()) == 0;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(this, obj))
                return true;

            if (obj is not MutableString str)
                return false;

            return Equals(this, str);
        }

        public bool Equals(MutableString obj) => Equals(this, obj);

        public static bool Equals(MutableString? a, MutableString? b)
        {
            if (a is null || b is null)
                return false;

            if (ReferenceEquals(a, b))
                return true;

            if (a.Length != b.Length)
                return false;

            return a.CompareInternal(b) == 0;
        }

        protected int CompareInternal(MutableString other) => _data.CompareCharArray((char[])other);

        public int CompareTo(object? obj)
        {
            if (obj is MutableString mutableString)
                return Compare(this, mutableString);

            return 1;
        }

        public int CompareTo(MutableString? value) => Compare(this, value);
        
        public int CompareTo(string? value)
        {
            if (value == null)
                return 1;

            return _data.CompareCharArray(value.ToCharArray());
        }

        public static int Compare(MutableString? a, MutableString? b)
        {
            if (ReferenceEquals(a, b))
                return 0;

            if (a == null)
                return -1;

            if (b == null)
                return 1;

            return a.CompareInternal(b);
        }

        public bool Contains(MutableString value) => Contains((char[])value);
        public bool Contains(char[] valueArray) => IndexOf(valueArray) >= 0;
        public bool Contains(string value) => Contains(value.ToCharArray());
        public static bool operator ==(MutableString a, MutableString b) => Equals(a, b);
        public static bool operator !=(MutableString a, MutableString b) => !Equals(a, b);
        public static bool operator <(MutableString a, MutableString b) => Compare(a, b) < 0;
        public static bool operator >(MutableString a, MutableString b) => Compare(a, b) > 0;
        public static bool operator <=(MutableString a, MutableString b) => Compare(a, b) <= 0;
        public static bool operator >=(MutableString a, MutableString b) => Compare(a, b) >= 0;
        public static implicit operator string(MutableString value) => value.ToString();
        public static explicit operator char[](MutableString value) => value._data;

        public static MutableString operator +(MutableString a, MutableString b)
        {
            var result = new MutableString(a.Length + b.Length);
            result.SetSubString(0, (char[])a);
            result.SetSubString(a.Length, (char[])b);
            return result;
        }
        
        public TypeCode GetTypeCode() => TypeCode.String;
        public bool ToBoolean(IFormatProvider? provider) => Convert.ToBoolean(ToString(), provider);
        public byte ToByte(IFormatProvider? provider) => Convert.ToByte(ToString(), provider);
        public char ToChar(IFormatProvider? provider) => Convert.ToChar(ToString(), provider);
        public DateTime ToDateTime(IFormatProvider? provider) => Convert.ToDateTime(ToString(), provider);
        public decimal ToDecimal(IFormatProvider? provider) => Convert.ToDecimal(ToString(), provider);
        public double ToDouble(IFormatProvider? provider) => Convert.ToDouble(ToString(), provider);
        public short ToInt16(IFormatProvider? provider) => Convert.ToInt16(ToString(), provider);
        public int ToInt32(IFormatProvider? provider) => Convert.ToInt32(ToString(), provider);
        public long ToInt64(IFormatProvider? provider) => Convert.ToInt64(ToString(), provider);
        public sbyte ToSByte(IFormatProvider? provider) => Convert.ToSByte(ToString(), provider);
        public float ToSingle(IFormatProvider? provider) => Convert.ToSingle(ToString(), provider);
        public string ToString(IFormatProvider? provider) => ToString();
        public object ToType(Type conversionType, IFormatProvider? provider) => Convert.ChangeType((IConvertible)ToString(), conversionType, provider);
        public ushort ToUInt16(IFormatProvider? provider) => Convert.ToUInt16(ToString(), provider);
        public uint ToUInt32(IFormatProvider? provider) => Convert.ToUInt32(ToString(), provider);
        public ulong ToUInt64(IFormatProvider? provider) => Convert.ToUInt64(ToString(), provider);

        public MutableString Lower()
        {
            _data.ToLower();
            return this;
        }

        public MutableString Upper()
        {
            _data.ToUpper();
            return this;
        }

        public MutableString ToLower() => ToString().ToMutable().Lower();
        public MutableString ToUpper() => ToString().ToMutable().Upper();
        public IEnumerable<MutableString> Split(char separator) => _data.Split(separator).ToMutable();

        private void Trim(bool head, bool tail)
        {
            lock (_lock)
            {
                var end   = Length - 1;
                var start = 0;

                if (head)
                {
                    for (start = 0; start < Length; start++)
                        if (!char.IsWhiteSpace(this[start]))
                            break;
                }

                if (tail)
                {
                    for (end = Length - 1; end >= start; end--)
                        if (!char.IsWhiteSpace(this[end]))
                            break;
                }

                var len    = end - start + 1;
                var buffer = new char[len];
                Array.Copy(_data, start, buffer, 0, len);
                _data  = buffer;
                _dirty = true;
            }
        }

        public MutableString Trim()
        {
            Trim(true, true);
            return this;
        }

        public MutableString TrimStart()
        {
            Trim(true, false);
            return this;
        }

        public MutableString TrimEnd()
        {
            Trim(false, true);
            return this;
        }
    }
}