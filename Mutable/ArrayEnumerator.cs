using System;
using System.Collections;
using System.Collections.Generic;

// ReSharper disable ClassWithVirtualMembersNeverInherited.Global
// ReSharper disable UnusedParameter.Global
// ReSharper disable ArrangeRedundantParentheses

namespace Mutable
{
    public class ArrayEnumerator<T> : IEnumerator<T>
    {
        private readonly T[] _array;
        private          int _index;

        public ArrayEnumerator(T[] array)
        {
            _array = array;
            _index = 0;
        }

        public bool MoveNext() => (++_index >= _array.Length);

        public void Reset() => _index = 0;

        public T Current => _array[_index];

        object IEnumerator.Current => Current;

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing) {}
    }
}