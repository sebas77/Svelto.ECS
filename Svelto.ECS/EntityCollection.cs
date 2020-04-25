using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using Svelto.DataStructures;

namespace Svelto.ECS
{
    public struct EntityCollection<T> where T : IEntityComponent
    {
        public EntityCollection(T[] array, uint count) : this()
        {
            _buffer.Set(array, count);
            _count = count;
        }

        public EntityCollection(MB<T> buffer, uint count)
        {
            _buffer = buffer;
            _count  = count;
        }

        public uint count => _count;

        readonly MB<T> _buffer;
        readonly uint  _count;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T[] ToFastAccess(out uint actualCount)
        {
            actualCount = _count;
            return _buffer.ToManagedArray();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public NB<NT> ToNativeBuffer<NT>() where NT : unmanaged, T
        {
            return new NB<NT>(_buffer.Pin(), _count, _buffer.capacity);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public MB<T> ToBuffer()
        {
            return _buffer;
        }

        public ref T this[uint i]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => ref _buffer[i];
        }

        public ref T this[int i]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => ref _buffer[i];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public EntityIterator GetEnumerator() { return new EntityIterator(_buffer, _count); }

        public struct EntityIterator
        {
            public EntityIterator(MB<T> array, uint count) : this()
            {
                _array = array.ToManagedArray();
                _count = count;
                _index = -1;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public bool MoveNext() { return ++_index < _count; }

            public ref T Current
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => ref _array[_index];
            }

            readonly T[]  _array;
            readonly uint _count;
            int           _index;
        }
    }

    public struct EntityCollection<T1, T2>
        where T1 : IEntityComponent where T2 : IEntityComponent
    {
        public EntityCollection(in EntityCollection<T1> array1, in EntityCollection<T2> array2)
        {
            _array1 = array1;
            _array2 = array2;
        }

        public uint count => _array1.count;
        
        public EntityCollection<T2> Item2
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _array2;
        }

        public EntityCollection<T1> Item1
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _array1;
        }

        readonly EntityCollection<T1> _array1;
        readonly EntityCollection<T2> _array2;

        public (T1[], T2[]) ToFastAccess(out uint count)
        {
            count = this.count;

            return (_array1.ToFastAccess(out _), _array2.ToFastAccess(out _));
        }

        public BT<MB<T1>, MB<T2>> ToBuffers()
        {
            var bufferTuple = new BT<MB<T1>, MB<T2>>
                (_array1.ToBuffer(), _array2.ToBuffer(), count);
            return bufferTuple;
        }

        public BT<NB<NT1>, NB<NT2>> ToNativeBuffers<NT1, NT2>()
            where NT2 : unmanaged, T2 where NT1 : unmanaged, T1
        {
            var bufferTuple = new BT<NB<NT1>, NB<NT2>>
                (_array1.ToNativeBuffer<NT1>(), _array2.ToNativeBuffer<NT2>(), count);

            return bufferTuple;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public EntityIterator GetEnumerator()
        {
            return new EntityIterator(this);
        }

        public struct EntityIterator
        {
            public EntityIterator(in EntityCollection<T1, T2> array1) : this()
            {
                _array1 = array1;
                _count = array1.count;
                _index = -1;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public bool MoveNext()
            {
                return ++_index < _count;
            }

            public void Reset()
            {
                _index = -1;
            }

            public ValueRef<T1, T2> Current
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => new ValueRef<T1, T2>(_array1, (uint) _index);
            }

            readonly EntityCollection<T1, T2> _array1;
            readonly uint                 _count;
            int                           _index;
        }
    }

    public struct EntityCollection<T1, T2, T3> 
        where T3 : IEntityComponent where T2 : IEntityComponent where T1 : IEntityComponent
    {
        public EntityCollection(
            in EntityCollection<T1> array1, in EntityCollection<T2> array2,
            in EntityCollection<T3> array3)
        {
            _array1 = array1;
            _array2 = array2;
            _array3 = array3;
        }

        public EntityCollection<T1> Item1
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _array1;
        }

        public EntityCollection<T2> Item2
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _array2;
        }

        public EntityCollection<T3> Item3
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _array3;
        }

        public uint count => Item1.count;

        public (T1[], T2[], T3[]) ToFastAccess(out uint count)
        {
            count = this.count;

            return (_array1.ToFastAccess(out _), _array2.ToFastAccess(out _), _array3.ToFastAccess(out _));
        }

        public BT<MB<T1>, MB<T2>, MB<T3>> ToBuffers()
        {
            var bufferTuple = new BT<MB<T1>, MB<T2>, MB<T3>>
                (_array1.ToBuffer(), _array2.ToBuffer(), _array3.ToBuffer(), count);
            return bufferTuple;
        }

        public BT<NB<NT1>, NB<NT2>, NB<NT3>> ToNativeBuffers<NT1, NT2, NT3>()
            where NT2 : unmanaged, T2 where NT1 : unmanaged, T1 where NT3 : unmanaged, T3
        {
            var bufferTuple = new BT<NB<NT1>, NB<NT2>, NB<NT3>>
            (_array1.ToNativeBuffer<NT1>(), _array2.ToNativeBuffer<NT2>(), _array3.ToNativeBuffer<NT3>(), count);

            return bufferTuple;
        }

        readonly EntityCollection<T1> _array1;
        readonly EntityCollection<T2> _array2;
        readonly EntityCollection<T3> _array3;
    }

    public struct EntityCollections<T> where T : struct, IEntityComponent
    {
        public EntityCollections(EntitiesDB db, ExclusiveGroup[] groups) : this()
        {
            _db = db;
            _groups = groups;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public EntityGroupsIterator GetEnumerator()
        {
            return new EntityGroupsIterator(_db, _groups);
        }

        readonly EntitiesDB      _db;
        readonly ExclusiveGroup[] _groups;

        public struct EntityGroupsIterator
        {
            public EntityGroupsIterator(EntitiesDB db, ExclusiveGroup[] groups) : this()
            {
                _db = db;
                _groups = groups;
                _indexGroup = -1;
                _index = -1;
            }

            public bool MoveNext()
            {
                //attention, the while is necessary to skip empty groups
                while (_index + 1 >= _count && ++_indexGroup < _groups.Length)
                {
                    _index = -1;
                    _array = _db.QueryEntities<T>(_groups[_indexGroup]);
                    _count = _array.count;
                }

                return ++_index < _count;
            }

            public void Reset()
            {
                _index = -1;
                _indexGroup = -1;
                _count = 0;
            }

            public ref T Current => ref _array[(uint) _index];

            readonly EntitiesDB      _db;
            readonly ExclusiveGroup[] _groups;

            EntityCollection<T> _array;
            uint                _count;
            int                 _index;
            int                 _indexGroup;
        }
    }

    public struct EntityCollections<T1, T2>
        where T1 : struct, IEntityComponent where T2 : struct, IEntityComponent
    {
        public EntityCollections(EntitiesDB db, ExclusiveGroup[] groups) : this()
        {
            _db = db;
            _groups = groups;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public EntityGroupsIterator GetEnumerator()
        {
            return new EntityGroupsIterator(_db, _groups);
        }

        readonly EntitiesDB      _db;
        readonly ExclusiveGroup[] _groups;

        public struct EntityGroupsIterator
        {
            public EntityGroupsIterator(EntitiesDB db, ExclusiveGroup[] groups) : this()
            {
                _db = db;
                _groups = groups;
                _indexGroup = -1;
                _index = -1;
            }

            public bool MoveNext()
            {
                //attention, the while is necessary to skip empty groups
                while (_index + 1 >= _array1.count && ++_indexGroup < _groups.Length)
                {
                    _index = -1;
                    _array1 = _db.QueryEntities<T1, T2>(_groups[_indexGroup]);
                }

                return ++_index < _array1.count;
            }

            public void Reset()
            {
                _index = -1;
                _indexGroup = -1;

                _array1 = _db.QueryEntities<T1, T2>(_groups[0]);
            }

            public ValueRef<T1, T2> Current
            {
                get
                {
                    var valueRef =
                        new ValueRef<T1, T2>(_array1, (uint) _index);
                    return valueRef;
                }
            }

            readonly EntitiesDB      _db;
            readonly ExclusiveGroup[] _groups;
            int                       _index;
            int                       _indexGroup;

            EntityCollection<T1, T2> _array1;
        }
    }
    
    public struct EntityCollections<T1, T2, T3>
        where T1 : struct, IEntityComponent where T2 : struct, IEntityComponent where T3 : struct, IEntityComponent
    {
        public EntityCollections(EntitiesDB db, ExclusiveGroup[] groups) : this()
        {
            _db = db;
            _groups = groups;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public EntityGroupsIterator GetEnumerator()
        {
            return new EntityGroupsIterator(_db, _groups);
        }

        readonly EntitiesDB      _db;
        readonly ExclusiveGroup[] _groups;

        public struct EntityGroupsIterator
        {
            public EntityGroupsIterator(EntitiesDB db, ExclusiveGroup[] groups) : this()
            {
                _db = db;
                _groups = groups;
                _indexGroup = -1;
                _index = -1;
            }

            public bool MoveNext()
            {
                //attention, the while is necessary to skip empty groups
                while (_index + 1 >= _count && ++_indexGroup < _groups.Length)
                {
                    _index = -1;
                    _array1 = _db.QueryEntities<T1, T2, T3>(_groups[_indexGroup]);
                    _count = _array1.count;

                }

                return ++_index < _count;
            }

            public void Reset()
            {
                _index = -1;
                _indexGroup = -1;

                _array1 = _db.QueryEntities<T1, T2, T3>(_groups[0]);
                _count = _array1.count;
            }

            public ValueRef<T1, T2, T3> Current
            {
                get
                {
                    var valueRef =
                        new ValueRef<T1, T2, T3>(_array1, (uint) _index);
                    return valueRef;
                }
            }

            readonly EntitiesDB      _db;
            readonly ExclusiveGroup[] _groups;
            uint                      _count;
            int                       _index;
            int                       _indexGroup;

            EntityCollection<T1, T2, T3> _array1;
        }
    }
    
    public readonly struct BT<BufferT1, BufferT2, BufferT3, BufferT4> : IDisposable where BufferT1 : IDisposable
                                                                          where BufferT2 : IDisposable
                                                                          where BufferT3 : IDisposable
                                                                          where BufferT4 : IDisposable
    {
        public readonly BufferT1 buffer1;
        public readonly BufferT2 buffer2;
        public readonly BufferT3 buffer3;
        public readonly BufferT4 buffer4;
        public readonly uint     count;

        public BT(BufferT1 bufferT1, BufferT2 bufferT2, BufferT3 bufferT3, BufferT4 bufferT4, uint count) : this()
        {
            this.buffer1 = bufferT1;
            this.buffer2 = bufferT2;
            this.buffer3 = bufferT3;
            this.buffer4 = bufferT4;
            this.count   = count;
        }

        public void Dispose()
        {
            buffer1.Dispose();
            buffer2.Dispose();
            buffer3.Dispose();
            buffer4.Dispose();
        }
    }

    public readonly struct BT<BufferT1, BufferT2, BufferT3> : IDisposable where BufferT1 : IDisposable
                                                                                   where BufferT2 : IDisposable
                                                                                   where BufferT3 : IDisposable
    {
        public readonly BufferT1 buffer1;
        public readonly BufferT2 buffer2;
        public readonly BufferT3 buffer3;
        public readonly uint     count;

        public BT(BufferT1 bufferT1, BufferT2 bufferT2, BufferT3 bufferT3, uint count) : this()
        {
            this.buffer1 = bufferT1;
            this.buffer2 = bufferT2;
            this.buffer3 = bufferT3;
            this.count = count;
        }

        public void Dispose()
        {
            buffer1.Dispose();
            buffer2.Dispose();
            buffer3.Dispose();
        }
    }

    public readonly struct BT<BufferT1, BufferT2> : IDisposable
        where BufferT1 : IDisposable where BufferT2 : IDisposable
    {
        public readonly BufferT1 buffer1;
        public readonly BufferT2 buffer2;
        public readonly uint count;

        public BT(BufferT1 bufferT1, BufferT2 bufferT2, uint count) : this()
        {
            this.buffer1 = bufferT1;
            this.buffer2 = bufferT2;
            this.count = count;
        }

        public void Dispose()
        {
            buffer1.Dispose();
            buffer2.Dispose();
        }
    }

    public ref struct ValueRef<T1, T2> where T2 : IEntityComponent where T1 : IEntityComponent
    {
        readonly EntityCollection<T1, T2> array1;

        readonly uint index;

        public ValueRef(in EntityCollection<T1, T2> entity2, uint i)
        {
            array1 = entity2;
            index = i;
        }

        public ref T1 entityComponentA
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => ref array1.Item1[index];
        }

        public ref T2 entityComponentB
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => ref array1.Item2[index];
        }
    }

    public ref struct ValueRef<T1, T2, T3> 
        where T2 : IEntityComponent where T1 : IEntityComponent where T3 : IEntityComponent
    {
        readonly EntityCollection<T1, T2, T3> array1;

        readonly uint index;

        public ValueRef(in EntityCollection<T1, T2, T3> entity, uint i)
        {
            array1 = entity;
            index  = i;
        }

        public ref T1 entityComponentA
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => ref array1.Item1[index];
        }

        public ref T2 entityComponentB
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => ref array1.Item2[index];
        }
        
        public ref T3 entityComponentC
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => ref array1.Item3[index];
        }
    }
}