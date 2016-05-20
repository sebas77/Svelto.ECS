/**
 * Custom modified version of Michael Lyashenko's BetterList
 **/

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Svelto.DataStructures
{
    public struct FasterListEnumerator<T> : IEnumerator<T>
    {
        public T Current
        {
            get { return _current; }
        }

        object IEnumerator.Current
        {
            get { return _current; }
        }

        T IEnumerator<T>.Current
        {
            get { return _current; }
        }

        public FasterListEnumerator(T[] buffer, int size)
        {
            _size = size;
            _counter = 0;
            _buffer = buffer;
            _current = default(T);
        }

        public void Dispose()
        {
            _buffer = null;
        }

        bool IEnumerator.MoveNext()
        {
            return MoveNext();
        }

        void IEnumerator.Reset()
        {
            Reset();
        }

        public bool MoveNext()
        {
            if (_counter < _size)
            {
                _current = _buffer[_counter++];

                return true;
            }

            _current = default(T);

            return false;
        }

        public void Reset()
        {
            _counter = 0;
        }

        T[] _buffer;
        int _counter;
        int _size;
        T _current;
    }

    public struct FasterListEnumeratorCast<T, U> : IEnumerator<T> where T:U
    {
        public T Current
        {
            get { return (T)_buffer.Current; }
        }

        object IEnumerator.Current
        {
            get { return (T)_buffer.Current; }
        }

        T IEnumerator<T>.Current
        {
            get { return (T)_buffer.Current; }
        }

        public FasterListEnumeratorCast(FasterListEnumerator<U> buffer)
        {
            _buffer = buffer;
        }

        public void Dispose()
        {}

        bool IEnumerator.MoveNext()
        {
            return MoveNext();
        }

        void IEnumerator.Reset()
        {
            Reset();
        }

        public bool MoveNext()
        {
            return _buffer.MoveNext();
        }

        public void Reset()
        {
            _buffer.Reset();
        }

        FasterListEnumerator<U> _buffer;
    }

    public struct FasterReadOnlyList<T> : IList<T>
    {
        public FasterReadOnlyList(FasterList<T> list)
        {
            _list = list;
        }

        IEnumerator<T> IEnumerable<T>.GetEnumerator()
        {
            return GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public FasterListEnumerator<T> GetEnumerator()
        {
            return _list.GetEnumerator();
        }

        public void Add(T item)
        {
            throw new NotImplementedException();
        }

        public void Clear()
        {
            throw new NotImplementedException();
        }

        public bool Contains(T item)
        {
            return _list.Contains(item);
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            _list.CopyTo(array, arrayIndex);
        }

        public bool Remove(T item)
        {
            throw new NotImplementedException();
        }

        public int Count { get { return _list.Count; } }
        public bool IsReadOnly { get { return true; } }

        public int IndexOf(T item)
        {
            return _list.IndexOf(item);
        }

        public void Insert(int index, T item)
        {
            throw new NotImplementedException();
        }

        public void RemoveAt(int index)
        {
            throw new NotImplementedException();
        }

        public T this[int index] { get { return _list[index]; } set { throw new NotImplementedException(); } }

        readonly FasterList<T> _list;
    }

    public struct FasterReadOnlyListCast<T, U> : IList<U> where U:T
    {
        public FasterReadOnlyListCast(FasterList<T> list)
        {
            _list = list;
        }

        IEnumerator<U> IEnumerable<U>.GetEnumerator()
        {
            return GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public FasterListEnumeratorCast<U, T> GetEnumerator()
        {
            return new FasterListEnumeratorCast<U, T>(_list.GetEnumerator());
        }

        public void Add(U item)
        {
            throw new NotImplementedException();
        }

        public void Clear()
        {
            throw new NotImplementedException();
        }

        public bool Contains(U item)
        {
            return _list.Contains(item);
        }

        public void CopyTo(U[] array, int arrayIndex)
        {
            throw new NotImplementedException();
        }

        public bool Remove(U item)
        {
            throw new NotImplementedException();
        }

        public int Count { get { return _list.Count; } }
        public bool IsReadOnly { get { return true; } }

        public int IndexOf(U item)
        {
            return _list.IndexOf(item);
        }

        public void Insert(int index, U item)
        {
            throw new NotImplementedException();
        }

        public void RemoveAt(int index)
        {
            throw new NotImplementedException();
        }

        public U this[int index] { get { return (U)_list[index]; } set { throw new NotImplementedException(); } }

        readonly FasterList<T> _list;
        static public FasterList<T> DefaultList = new FasterList<T>();
    }

    public class FasterList<T> : IList<T>
    {
        public int Count
        {
            get { return _count; }
        }

        public bool IsReadOnly
        {
            get { return false; }
        }

        public FasterList()
        {
            _count = 0;

            _buffer = new T[MIN_SIZE];
        }

        public FasterList(int initialSize)
        {
            _count = 0;

            _buffer = new T[initialSize];
        }

        public FasterList(ICollection<T> collection)
        {
            _buffer = new T[collection.Count];

            collection.CopyTo(_buffer, 0);

            _count = _buffer.Length;
        }

        public FasterList(FasterList<T> listCopy)
        {
            _buffer = new T[listCopy.Count];

            listCopy.CopyTo(_buffer, 0);

            _count = listCopy.Count;
        }

        public void Add(T item)
        {
            if (_count == _buffer.Length)
                AllocateMore();

            _buffer[_count++] = item;
        }

        public void AddRange(IEnumerable<T> items, int count)
        {
            if (_count + count >= _buffer.Length)
                AllocateMore(_count + count);

            for (var i = items.GetEnumerator(); i.MoveNext();)
            {
                var item = i.Current;
                _buffer[_count++] = item;
            }
        }

        public void AddRange(ICollection<T> items)
        {
            var count = items.Count;
            if (_count + count >= _buffer.Length)
                AllocateMore(_count + count);

            for (var i = items.GetEnumerator(); i.MoveNext();)
            {
                var item = i.Current;
                _buffer[_count++] = item;
            }
        }

        public void AddRange(FasterList<T> items)
        {
            var count = items.Count;
            if (_count + count >= _buffer.Length)
                AllocateMore(_count + count);

            Array.Copy(items._buffer, 0, _buffer, _count, count);
            _count += count;
        }

        public void AddRange(T[] items)
        {
            var count = items.Length;
            if (_count + count >= _buffer.Length)
                AllocateMore(_count + count);

            Array.Copy(items, 0, _buffer, _count, count);
            _count += count;
        }

        public FasterReadOnlyList<T> AsReadOnly()
        {
            return new FasterReadOnlyList<T>(this);
        }

        public void Clear()
        {
            _count = 0;
        }

        public bool Contains(T item)
        {
            var index = IndexOf(item);

            return index != -1;
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            Array.Copy(_buffer, 0, array, arrayIndex, Count);
        }

        public FasterListEnumerator<T> GetEnumerator()
        {
            return new FasterListEnumerator<T>(_buffer, Count);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        IEnumerator<T> IEnumerable<T>.GetEnumerator()
        {
            return GetEnumerator();
        }

        public int IndexOf(T item)
        {
            var comp = EqualityComparer<T>.Default;

            for (var index = _count - 1; index >= 0; --index)
                if (comp.Equals(_buffer[index], item))
                    return index;

            return -1;
        }

        public void Insert(int index, T item)
        {
            DesignByContract.Check.Require(index < _count, "out of bound index");

            if (_count == _buffer.Length) AllocateMore();

            Array.Copy(_buffer, index, _buffer, index + 1, _count - index);

            _buffer[index] = item;
            ++_count;
        }

        public void Release()
        {
            _count = 0;
            _buffer = null;
        }

        public bool Remove(T item)
        {
            var index = IndexOf(item);

            if (index == -1)
                return false;

            RemoveAt(index);

            return true;
        }

        public void RemoveAt(int index)
        {
            DesignByContract.Check.Require(index < _count, "out of bound index");

            --_count;

            if (index == _count)
                return;

            Array.Copy(_buffer, index + 1, _buffer, index, _count - index);
        }

        public void Resize(int newSize)
        {
            Array.Resize(ref _buffer, newSize);

            _count = newSize;
        }

        public void SetAt(int index, T value)
        {
            if (index >= _buffer.Length)
                AllocateMore(index + 1);

            if (_count <= index)
                _count = index + 1;

            this[index] = value;
        }

        public void Sort(Comparison<T> comparer)
        {
            Trim();
            Array.Sort(_buffer, comparer);
        }

        public T[] ToArray()
        {
            T[] destinationArray = new T[_count];

            Array.Copy(_buffer, 0, destinationArray, 0, _count);

            return destinationArray;
        }

        /// <summary>
        /// This function exists to allow fast iterations. The size of the array returned cannot be
        /// used. The list count must be used instead.
        /// </summary>
        /// <returns></returns>
        public T[] ToArrayFast()
        {
            return _buffer;
        }

        public bool UnorderredRemove(T item)
        {
            var index = IndexOf(item);

            if (index == -1)
                return false;

            UnorderredRemoveAt(index);

            return true;
        }

        public void UnorderredRemoveAt(int index)
        {
            DesignByContract.Check.Require(index < _count, "out of bound index");

            _buffer[index] = _buffer[--_count];
        }

        private void AllocateMore()
        {
            var newList = new T[Mathf.Max(_buffer.Length << 1, MIN_SIZE)];
            if (_count > 0) _buffer.CopyTo(newList, 0);
            _buffer = newList;
        }

        private void AllocateMore(int newSize)
        {
            var oldLength = _buffer.Length;

            while (oldLength < newSize)
                oldLength <<= 1;

            var newList = new T[oldLength];
            if (_count > 0) _buffer.CopyTo(newList, 0);
            _buffer = newList;
        }

        private void Trim()
        {
            if (_count < _buffer.Length)
                Resize(_count);
        }

        public T this[int i]
        {
            get { DesignByContract.Check.Require(i < _count, "out of bound index"); return _buffer[i]; }
            set { DesignByContract.Check.Require(i < _count, "out of bound index"); _buffer[i] = value; }
        }

        private const int MIN_SIZE = 32;
        private T[] _buffer;
        private int _count;
    }
}
