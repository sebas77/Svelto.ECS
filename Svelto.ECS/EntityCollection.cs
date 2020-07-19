using System.Runtime.CompilerServices;
using Svelto.Common;
using Svelto.DataStructures;
using Svelto.ECS.Internal;

namespace Svelto.ECS
{
    public readonly ref struct EntityCollection<T> where T : struct, IEntityComponent
    {
        static readonly bool IsUnmanaged = TypeSafeDictionary<T>._isUmanaged; 
        
        public EntityCollection(IBuffer<T> buffer, uint count):this()
        {
            if (IsUnmanaged)
                _nativedBuffer = (NB<T>) buffer;
            else
                _managedBuffer = (MB<T>) buffer;
            
            _count  = count;
            _buffer = buffer;
        }

        public uint count => _count;

        internal readonly MB<T> _managedBuffer;
        internal readonly NB<T> _nativedBuffer;
        readonly uint       _count;

        //todo very likely remove this
        public ref T this[uint i]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                if (IsUnmanaged)
                    return ref _nativedBuffer[i];
                else
                    return ref _managedBuffer[i];
            }
        }

        public ref T this[int i]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                if (IsUnmanaged)
                    return ref _nativedBuffer[i];
                else
                    return ref _managedBuffer[i];
            }
        }

        //TODO SOON: ALL THIS STUFF BELOW MUST DISAPPEAR
        readonly IBuffer<T> _buffer;

        //todo to remove
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public EntityIterator GetEnumerator() { return new EntityIterator(_buffer, _count); }
//todo to remove
        public ref struct EntityIterator
        {
            public EntityIterator(IBuffer<T> array, uint count) : this()
            {
                _array = array;
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

            readonly IBuffer<T> _array;
            readonly uint       _count;
            int                 _index;
        }
    }

    public readonly ref struct EntityCollection<T1, T2> where T1 : struct, IEntityComponent where T2 : struct, IEntityComponent
    {
        public EntityCollection(in EntityCollection<T1> array1, in EntityCollection<T2> array2)
        {
            _array1 = array1;
            _array2 = array2;
        }

        public uint count => _array1.count;

        //todo to remove
        public EntityCollection<T2> Item2
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _array2;
        }
//todo to remove
        public EntityCollection<T1> Item1
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _array1;
        }

        readonly EntityCollection<T1> _array1;
        readonly EntityCollection<T2> _array2;
//todo to remove
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public EntityIterator GetEnumerator() { return new EntityIterator(this); }
//todo to remove
        public ref struct EntityIterator
        {
            public EntityIterator(in EntityCollection<T1, T2> array1) : this()
            {
                _array1 = array1;
                _count  = array1.count;
                _index  = -1;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public bool MoveNext() { return ++_index < _count; }

            public void Reset() { _index = -1; }

            public ValueRef<T1, T2> Current
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => new ValueRef<T1, T2>(_array1, (uint) _index);
            }

            readonly EntityCollection<T1, T2> _array1;
            readonly uint                     _count;
            int                               _index;
        }

    }

    public readonly ref struct EntityCollection<T1, T2, T3> where T3 : struct, IEntityComponent
                                                        where T2 : struct, IEntityComponent
                                                        where T1 : struct, IEntityComponent
    {
        public EntityCollection
            (in EntityCollection<T1> array1, in EntityCollection<T2> array2, in EntityCollection<T3> array3)
        {
            _array1 = array1;
            _array2 = array2;
            _array3 = array3;
        }
//todo to remove
        public EntityCollection<T1> Item1
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _array1;
        }
//todo to remove
        public EntityCollection<T2> Item2
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _array2;
        }
//todo to remove
        public EntityCollection<T3> Item3
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _array3;
        }

        public uint count => Item1.count;

        readonly EntityCollection<T1> _array1;
        readonly EntityCollection<T2> _array2;
        readonly EntityCollection<T3> _array3;
    }
    
    public readonly ref struct EntityCollection<T1, T2, T3, T4> 
                                                            where T1 : struct, IEntityComponent
                                                            where T2 : struct, IEntityComponent
                                                            where T3 : struct, IEntityComponent
                                                            where T4 : struct, IEntityComponent
    {
        public EntityCollection
            (in EntityCollection<T1> array1, in EntityCollection<T2> array2, in EntityCollection<T3> array3, in EntityCollection<T4> array4)
        {
            _array1 = array1;
            _array2 = array2;
            _array3 = array3;
            _array4 = array4;
        }

        //todo to remove
        public EntityCollection<T1> Item1
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _array1;
        }

        //todo to remove
        public EntityCollection<T2> Item2
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _array2;
        }

        //todo to remove
        public EntityCollection<T3> Item3
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _array3;
        }
        
        //todo to remove
        public EntityCollection<T4> Item4
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _array4;
        }

        public uint count => _array1.count;

        readonly EntityCollection<T1> _array1;
        readonly EntityCollection<T2> _array2;
        readonly EntityCollection<T3> _array3;
        readonly EntityCollection<T4> _array4;
    }

    public readonly struct BT<BufferT1, BufferT2, BufferT3, BufferT4>
    {
        public readonly BufferT1 buffer1;
        public readonly BufferT2 buffer2;
        public readonly BufferT3 buffer3;
        public readonly BufferT4 buffer4;
        public readonly int     count;

        public BT(BufferT1 bufferT1, BufferT2 bufferT2, BufferT3 bufferT3, BufferT4 bufferT4, uint count) : this()
        {
            this.buffer1 = bufferT1;
            this.buffer2 = bufferT2;
            this.buffer3 = bufferT3;
            this.buffer4 = bufferT4;
            this.count   = (int) count;
        }
    }

    public readonly struct BT<BufferT1, BufferT2, BufferT3>
    {
        public readonly BufferT1 buffer1;
        public readonly BufferT2 buffer2;
        public readonly BufferT3 buffer3;
        public readonly int     count;

        public BT(BufferT1 bufferT1, BufferT2 bufferT2, BufferT3 bufferT3, uint count) : this()
        {
            this.buffer1 = bufferT1;
            this.buffer2 = bufferT2;
            this.buffer3 = bufferT3;
            this.count   = (int) count;
        }

        public void Deconstruct(out BufferT1 bufferT1, out BufferT2 bufferT2, out BufferT3 bufferT3, out int count)
        {
            bufferT1 = buffer1;
            bufferT2 = buffer2;
            bufferT3 = buffer3;
            count = this.count;
        }
    }

    public readonly struct BT<BufferT1>
    {
        public readonly BufferT1 buffer;
        public readonly int     count;

        public BT(BufferT1 bufferT1, uint count) : this()
        {
            this.buffer = bufferT1;
            this.count  = (int) count;
        }
        
        public void Deconstruct(out BufferT1 bufferT1, out int count)
        {
            bufferT1 = buffer;
            count    = this.count;
        }
        
        public static implicit operator BufferT1(BT<BufferT1> t) => t.buffer;
    }

    public readonly struct BT<BufferT1, BufferT2>
    {
        public readonly BufferT1 buffer1;
        public readonly BufferT2 buffer2;
        public readonly int     count;

        public BT(BufferT1 bufferT1, BufferT2 bufferT2, uint count) : this()
        {
            this.buffer1 = bufferT1;
            this.buffer2 = bufferT2;
            this.count   = (int) count;
        }
        
        public void Deconstruct(out BufferT1 bufferT1, out BufferT2 bufferT2, out int count)
        {
            bufferT1 = buffer1;
            bufferT2 = buffer2;
            count    = this.count;
        }
    }

    public readonly ref struct ValueRef<T1, T2> where T2 : struct, IEntityComponent where T1 : struct, IEntityComponent
    {
        readonly EntityCollection<T1, T2> array1;

        readonly uint index;

        public ValueRef(in EntityCollection<T1, T2> entity2, uint i)
        {
            array1 = entity2;
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
    }

    public readonly ref struct ValueRef<T1, T2, T3> where T2 : struct, IEntityComponent
                                                    where T1 : struct, IEntityComponent
                                                    where T3 : struct, IEntityComponent
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
