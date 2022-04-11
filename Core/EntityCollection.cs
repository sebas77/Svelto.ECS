using System.Runtime.CompilerServices;
using Svelto.DataStructures;
using Svelto.ECS.Internal;

namespace Svelto.ECS
{
    public readonly ref struct EntityCollection<T> where T : struct, IEntityComponent
    {
        static readonly bool IsUnmanaged = TypeSafeDictionary<T>.isUnmanaged;

        public EntityCollection(IBuffer<T> buffer, uint count, EntityIDs entityIDs) : this()
        {
            DBC.ECS.Check.Require(count == 0 || buffer.isValid, "Buffer is found in impossible state");
            if (IsUnmanaged)
            {
                _nativedBuffer  = (NB<T>)buffer;
                _nativedIndices = entityIDs.nativeIDs;
            }
            else
            {
                _managedBuffer  = (MB<T>)buffer;
                _managedIndices = entityIDs.managedIDs;
            }

            this.count          = count;
        }
        
        public EntityCollection(IBuffer<T> buffer, uint count) : this()
        {
            DBC.ECS.Check.Require(count == 0 || buffer.isValid, "Buffer is found in impossible state");
            if (IsUnmanaged)
                _nativedBuffer = (NB<T>)buffer;
            else
                _managedBuffer = (MB<T>)buffer;

            this.count = count;
        }

        public uint count { get; }

        internal readonly MB<T> _managedBuffer;
        internal readonly NB<T> _nativedBuffer;
        
        internal readonly NativeEntityIDs  _nativedIndices;
        internal readonly ManagedEntityIDs _managedIndices;
    }

    public readonly ref struct EntityCollection<T1, T2> where T1 : struct, IEntityComponent
        where T2 : struct, IEntityComponent
    {
        internal EntityCollection(in EntityCollection<T1> array1, in EntityCollection<T2> array2)
        {
            buffer1 = array1;
            buffer2 = array2;
        }

        public int count => (int)buffer1.count;

        internal EntityCollection<T2> buffer2
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get;
        }

        internal EntityCollection<T1> buffer1
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get;
        }
    }

    public readonly ref struct EntityCollection<T1, T2, T3> where T3 : struct, IEntityComponent
        where T2 : struct, IEntityComponent
        where T1 : struct, IEntityComponent
    {
        internal EntityCollection(in EntityCollection<T1> array1, in EntityCollection<T2> array2,
            in EntityCollection<T3> array3)
        {
            buffer1 = array1;
            buffer2 = array2;
            buffer3 = array3;
        }

        internal EntityCollection<T1> buffer1
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get;
        }

        internal EntityCollection<T2> buffer2
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get;
        }

        internal EntityCollection<T3> buffer3
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get;
        }

        internal int count => (int)buffer1.count;
    }

    public readonly ref struct EntityCollection<T1, T2, T3, T4> where T1 : struct, IEntityComponent
        where T2 : struct, IEntityComponent
        where T3 : struct, IEntityComponent
        where T4 : struct, IEntityComponent
    {
        internal EntityCollection(in EntityCollection<T1> array1, in EntityCollection<T2> array2,
            in EntityCollection<T3> array3, in EntityCollection<T4> array4)
        {
            buffer1 = array1;
            buffer2 = array2;
            buffer3 = array3;
            buffer4 = array4;
        }

        internal EntityCollection<T1> buffer1
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get;
        }

        internal EntityCollection<T2> buffer2
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get;
        }

        internal EntityCollection<T3> buffer3
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get;
        }

        internal EntityCollection<T4> buffer4
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get;
        }

        internal int count => (int)buffer1.count;
    }

    public readonly struct BT<BufferT1, BufferT2, BufferT3, BufferT4>
    {
        public readonly BufferT1 buffer1;
        public readonly BufferT2 buffer2;
        public readonly BufferT3 buffer3;
        public readonly BufferT4 buffer4;
        public readonly int      count;

        BT(in (BufferT1 bufferT1, BufferT2 bufferT2, BufferT3 bufferT3, BufferT4 bufferT4, int count) buffer) :
            this()
        {
            buffer1 = buffer.bufferT1;
            buffer2 = buffer.bufferT2;
            buffer3 = buffer.bufferT3;
            buffer4 = buffer.bufferT4;
            count   = buffer.count;
        }

        public static implicit operator BT<BufferT1, BufferT2, BufferT3, BufferT4>(
            in (BufferT1 bufferT1, BufferT2 bufferT2, BufferT3 bufferT3, BufferT4 bufferT4, int count) buffer)
        {
            return new BT<BufferT1, BufferT2, BufferT3, BufferT4>(buffer);
        }
    }

    public readonly struct BT<BufferT1, BufferT2, BufferT3>
    {
        public readonly BufferT1 buffer1;
        public readonly BufferT2 buffer2;
        public readonly BufferT3 buffer3;
        public readonly int      count;

        BT(in (BufferT1 bufferT1, BufferT2 bufferT2, BufferT3 bufferT3, int count) buffer) : this()
        {
            buffer1 = buffer.bufferT1;
            buffer2 = buffer.bufferT2;
            buffer3 = buffer.bufferT3;
            count   = buffer.count;
        }

        public static implicit operator BT<BufferT1, BufferT2, BufferT3>(
            in (BufferT1 bufferT1, BufferT2 bufferT2, BufferT3 bufferT3, int count) buffer)
        {
            return new BT<BufferT1, BufferT2, BufferT3>(buffer);
        }
    }

    public readonly struct BT<BufferT1>
    {
        public readonly BufferT1 buffer;
        public readonly int      count;

        BT(in (BufferT1 bufferT1, int count) buffer) : this()
        {
            this.buffer = buffer.bufferT1;
            count  = buffer.count;
        }

        public static implicit operator BT<BufferT1>(in (BufferT1 bufferT1, int count) buffer)
        {
            return new BT<BufferT1>(buffer);
        }

        public static implicit operator BufferT1(BT<BufferT1> t) => t.buffer;
    }

    public readonly struct BT<BufferT1, BufferT2>
    {
        public readonly BufferT1 buffer1;
        public readonly BufferT2 buffer2;
        public readonly int      count;

        BT(in (BufferT1 bufferT1, BufferT2 bufferT2, int count) buffer) : this()
        {
            buffer1 = buffer.bufferT1;
            buffer2 = buffer.bufferT2;
            count   = buffer.count;
        }

        public static implicit operator BT<BufferT1, BufferT2>(
            in (BufferT1 bufferT1, BufferT2 bufferT2, int count) buffer)
        {
            return new BT<BufferT1, BufferT2>(buffer);
        }
    }
}