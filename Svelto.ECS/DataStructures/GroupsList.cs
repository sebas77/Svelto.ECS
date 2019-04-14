using System;
using System.Runtime.CompilerServices;
using Svelto.DataStructures;

namespace Svelto.ECS.Internal
{
    class GroupList<T>
    {
        public int  Count => _list.Count;

        public GroupList()
        {
            _list = new FasterList<T>();
        }

        public ref T this[uint index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => ref _list[index];
        }

        public GroupListEnumerator<T> GetEnumerator()
        {
            return new GroupListEnumerator<T>(_list.ToArrayFast(), _list.Count);
        }

        public T GetOrAdd<TC>(uint location) where TC:class, T, new()
        {
            if (location >= _list.Count || this[location] == null)
            {
                var item = new TC();
                _list.Add(location, item);
                return item;
            }

            return this[location];
        }
        
        public void Clear() { _list.Clear(); }
        public void FastClear() { _list.ResetCountToAvoidGC(); }
        
        public bool TryGetValue(uint index, out T value)
        {
            if (default(T) == null)
            {
                if (index < _list.Count && this[index] != null)
                {
                    value = this[index];
                    return true;
                }

                value = default(T);
                return false;
            }
            else
            {
                    if (index < _list.Count)
                    {
                        value = this[index];
                        return true;
                    }

                    value = default(T);
                    return false;
            }
        }
        
        public void Add(uint location, T value) { _list.Add(location, value); }

        readonly FasterList<T> _list;
        
    }
    
    public struct GroupListEnumerator<T>
    {
        public ref readonly T Current => ref _buffer[_counter  -1];
        public uint index => _counter - 1;

        public GroupListEnumerator(T[] buffer, int size)
        {
            _size = size;
            _counter = 0;
            _buffer = buffer;
        }

        public bool MoveNext()
        {
            if (default(T) == null)
            {
                while (_counter < _size)
                {
                    if (_buffer[_counter] == null)
                        _counter++;
                    else
                        break;
                }
            }

            return _counter++ < _size;
        }

        readonly T[] _buffer;
        uint         _counter;
        readonly int _size;
    } 
}