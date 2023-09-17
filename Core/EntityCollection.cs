using System.Runtime.CompilerServices;
using Svelto.DataStructures;
using Svelto.ECS.Internal;

namespace Svelto.ECS
{
    public readonly ref struct EntityCollection<T> where T : struct, _IInternalEntityComponent
    {
        public EntityCollection(IBuffer<T> buffer, IEntityIDs entityIDs, uint count) : this()
        {
            DBC.ECS.Check.Require(count == 0 || buffer.isValid, "Buffer is found in impossible state");

            this.buffer    = buffer;
            this.entityIDs = entityIDs;
            this.count = count;
        }

        public uint count { get; }

        public readonly IBufferBase buffer;
        public readonly IEntityIDs  entityIDs;
    }

    public readonly ref struct EntityCollection<T1, T2> where T1 : struct, _IInternalEntityComponent
                                                        where T2 : struct, _IInternalEntityComponent
    {
        internal EntityCollection(in EntityCollection<T1> array1, in EntityCollection<T2> array2)
        {
            buffer1 = array1;
            buffer2 = array2;
        }

        public int count => (int)buffer1.count;

        public EntityCollection<T2> buffer2
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get;
        }

        public EntityCollection<T1> buffer1
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get;
        }
    }

    public readonly ref struct EntityCollection<T1, T2, T3> where T3 : struct, _IInternalEntityComponent
                                                            where T2 : struct, _IInternalEntityComponent
                                                            where T1 : struct, _IInternalEntityComponent
    {
        internal EntityCollection
            (in EntityCollection<T1> array1, in EntityCollection<T2> array2, in EntityCollection<T3> array3)
        {
            buffer1 = array1;
            buffer2 = array2;
            buffer3 = array3;
        }

        public EntityCollection<T1> buffer1
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get;
        }

        public EntityCollection<T2> buffer2
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get;
        }

        public EntityCollection<T3> buffer3
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get;
        }

        public int count => (int)buffer1.count;
    }

    public readonly ref struct EntityCollection<T1, T2, T3, T4> where T1 : struct, _IInternalEntityComponent
                                                                where T2 : struct, _IInternalEntityComponent
                                                                where T3 : struct, _IInternalEntityComponent
                                                                where T4 : struct, _IInternalEntityComponent
    {
        internal EntityCollection
        (in EntityCollection<T1> array1, in EntityCollection<T2> array2, in EntityCollection<T3> array3
       , in EntityCollection<T4> array4)
        {
            buffer1 = array1;
            buffer2 = array2;
            buffer3 = array3;
            buffer4 = array4;
        }

        public EntityCollection<T1> buffer1
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get;
        }

        public EntityCollection<T2> buffer2
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get;
        }

        public EntityCollection<T3> buffer3
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get;
        }

        public EntityCollection<T4> buffer4
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get;
        }

        public int count => (int)buffer1.count;
    }
}