using System;
using System.Collections;
using System.Collections.Generic;

namespace Svelto.DataStructures
{
    // Serves as simple circular buffer dictionary, first in, first out
    // Main drawback: it is the oldest in the list that is removed and the fact that we might re access a key
    // isn't taken into account (we would have to do a shift in both arrays)
    // Could be added as an option?

    public class CircularBufferIndexer<TKey, TVal> : IDictionary<TKey, TVal>
    {
        public ICollection<TKey> Keys
        {
            get { return _keys; }
        }

        public ICollection<TVal> Values
        {
            get { return _values; }
        }

        public int Count
        {
            get { throw new NotImplementedException(); }
        }

        public bool IsReadOnly
        {
            get { throw new NotImplementedException(); }
        }

        public CircularBufferIndexer(int size)
        {
            _keys = new TKey[size];
            _values = new TVal[size];
            _length = _startIndex = _nextIndex = 0;
        }

        public TVal this[TKey key]
        {
            get
            {
                int index = _startIndex;
                for (int i = 0; i < _length; ++i)
                {
                    if (_keys[index].Equals(key))
                    {
                        return _values[index];
                    }

                    index = NextPosition(index);
                }
                throw new KeyNotFoundException();
            }
            set
            {
                int index = _startIndex;
                for (int i = 0; i < _length; ++i)
                {
                    if (_keys[index].Equals(key))
                    {
                        _values[index] = value;
                        return;
                    }

                    index = NextPosition(index);
                }
                throw new KeyNotFoundException();
            }
        }

        public void Add(TKey key, TVal value)
        {
            if (ContainsKey(key))
            {
                this[key] = value;
                return;
            }

            _keys[_nextIndex] = key;
            _values[_nextIndex] = value;
            _nextIndex = NextPosition(_nextIndex);
            if (IsFull())
            {
                _startIndex = NextPosition(_startIndex);
            }
            else
            {
                ++_length;
            }
        }

        public bool ContainsKey(TKey key)
        {
            int index = _startIndex;
            for (int i = 0; i < _length; ++i)
            {
                if (_keys[index].Equals(key))
                {
                    return true;
                }

                index = NextPosition(index);
            }
            return false;
        }

        public bool Remove(TKey key)
        {
            throw new NotImplementedException();
        }

        public bool TryGetValue(TKey key, out TVal value)
        {
            value = default(TVal);
            int index = _startIndex;
            for (int i = 0; i < _length; ++i)
            {
                if (_keys[index].Equals(key))
                {
                    value = _values[index];
                    return true;
                }

                index = NextPosition(index);
            }
            return false;
        }

        public IEnumerator GetEnumerator()
        {
            throw new NotImplementedException();
        }

        public void Add(KeyValuePair<TKey, TVal> item)
        {
            Add(item.Key, item.Value);
        }

        public void Clear()
        {
            throw new NotImplementedException();
        }

        public bool Contains(KeyValuePair<TKey, TVal> item)
        {
            throw new NotImplementedException();
        }

        public void CopyTo(KeyValuePair<TKey, TVal>[] array, int arrayIndex)
        {
            throw new NotImplementedException();
        }

        public bool Remove(KeyValuePair<TKey, TVal> item)
        {
            throw new NotImplementedException();
        }

        IEnumerator<KeyValuePair<TKey, TVal>> IEnumerable<KeyValuePair<TKey, TVal>>.GetEnumerator()
        {
            throw new NotImplementedException();
        }

        int NextPosition(int position)
        {
            return (position + 1) % _keys.Length;
        }

        bool IsFull()
        {
            return _length == _values.Length;
        }

        TKey[] _keys;
        TVal[] _values;
        int _startIndex;
        int _nextIndex;
        int _length;
    }
}
