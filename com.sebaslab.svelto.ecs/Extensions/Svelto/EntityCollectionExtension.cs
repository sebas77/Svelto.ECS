using System.Runtime.CompilerServices;
using Svelto.DataStructures;
using Svelto.ECS.Hybrid;
using Svelto.ECS.Internal;

namespace Svelto.ECS
{
    public static class EntityCollectionExtension
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Deconstruct<T1>(in this EntityCollection<T1> ec, out NB<T1> buffer, out int count)
            where T1 : unmanaged, IEntityComponent
        {
            buffer = ec._nativedBuffer;
            count  = (int)ec.count;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Deconstruct<T1>(in this EntityCollection<T1> ec, out NB<T1> buffer,
            out NativeEntityIDs entityIDs, out int count) where T1 : unmanaged, IEntityComponent
        {
            buffer    = ec._nativedBuffer;
            count     = (int)ec.count;
            entityIDs = ec._nativedIndices;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Deconstruct<T1, T2>(in this EntityCollection<T1, T2> ec, out NB<T1> buffer1,
            out NB<T2> buffer2, out NativeEntityIDs entityIDs, out int count) where T1 : unmanaged, IEntityComponent
            where T2 : unmanaged, IEntityComponent
        {
            buffer1   = ec.buffer1._nativedBuffer;
            buffer2   = ec.buffer2._nativedBuffer;
            count     = ec.count;
            entityIDs = ec.buffer1._nativedIndices;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Deconstruct<T1, T2>(in this EntityCollection<T1, T2> ec, out NB<T1> buffer1,
            out NB<T2> buffer2, out int count) where T1 : unmanaged, IEntityComponent
            where T2 : unmanaged, IEntityComponent
        {
            buffer1 = ec.buffer1._nativedBuffer;
            buffer2 = ec.buffer2._nativedBuffer;
            count   = (int)ec.count;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Deconstruct<T1, T2, T3>(in this EntityCollection<T1, T2, T3> ec, out NB<T1> buffer1,
            out NB<T2> buffer2, out NB<T3> buffer3, out int count) where T1 : unmanaged, IEntityComponent
            where T2 : unmanaged, IEntityComponent
            where T3 : unmanaged, IEntityComponent
        {
            buffer1 = ec.buffer1._nativedBuffer;
            buffer2 = ec.buffer2._nativedBuffer;
            buffer3 = ec.buffer3._nativedBuffer;
            count   = (int)ec.count;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Deconstruct<T1, T2, T3>(in this EntityCollection<T1, T2, T3> ec, out NB<T1> buffer1,
            out NB<T2> buffer2, out NB<T3> buffer3, out NativeEntityIDs entityIDs, out int count)
            where T1 : unmanaged, IEntityComponent
            where T2 : unmanaged, IEntityComponent
            where T3 : unmanaged, IEntityComponent
        {
            buffer1   = ec.buffer1._nativedBuffer;
            buffer2   = ec.buffer2._nativedBuffer;
            buffer3   = ec.buffer3._nativedBuffer;
            count     = (int)ec.count;
            entityIDs = ec.buffer1._nativedIndices;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Deconstruct<T1, T2, T3, T4>(in this EntityCollection<T1, T2, T3, T4> ec, out NB<T1> buffer1,
            out NB<T2> buffer2, out NB<T3> buffer3, out NB<T4> buffer4, out int count)
            where T1 : unmanaged, IEntityComponent
            where T2 : unmanaged, IEntityComponent
            where T3 : unmanaged, IEntityComponent
            where T4 : unmanaged, IEntityComponent
        {
            buffer1 = ec.buffer1._nativedBuffer;
            buffer2 = ec.buffer2._nativedBuffer;
            buffer3 = ec.buffer3._nativedBuffer;
            buffer4 = ec.buffer4._nativedBuffer;
            count   = (int)ec.count;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Deconstruct<T1, T2, T3, T4>(in this EntityCollection<T1, T2, T3, T4> ec, out NB<T1> buffer1,
            out NB<T2> buffer2, out NB<T3> buffer3, out NB<T4> buffer4, out NativeEntityIDs entityIDs, out int count)
            where T1 : unmanaged, IEntityComponent
            where T2 : unmanaged, IEntityComponent
            where T3 : unmanaged, IEntityComponent
            where T4 : unmanaged, IEntityComponent
        {
            buffer1   = ec.buffer1._nativedBuffer;
            buffer2   = ec.buffer2._nativedBuffer;
            buffer3   = ec.buffer3._nativedBuffer;
            buffer4   = ec.buffer4._nativedBuffer;
            entityIDs = ec.buffer1._nativedIndices;
            count     = (int)ec.count;
        }
    }

    public static class EntityCollectionExtensionB
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Deconstruct<T1>(in this EntityCollection<T1> ec, out MB<T1> buffer, out int count)
            where T1 : struct, IEntityViewComponent
        {
            buffer = ec._managedBuffer;
            count  = (int)ec.count;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Deconstruct<T1>(in this EntityCollection<T1> ec, out MB<T1> buffer,
            out ManagedEntityIDs entityIDs, out int count) where T1 : struct, IEntityViewComponent
        {
            buffer    = ec._managedBuffer;
            count     = (int)ec.count;
            entityIDs = ec._managedIndices;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Deconstruct<T1, T2>(in this EntityCollection<T1, T2> ec, out MB<T1> buffer1,
            out MB<T2> buffer2, out int count) where T1 : struct, IEntityViewComponent
            where T2 : struct, IEntityViewComponent
        {
            buffer1 = ec.buffer1._managedBuffer;
            buffer2 = ec.buffer2._managedBuffer;
            count   = (int)ec.count;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Deconstruct<T1, T2>(in this EntityCollection<T1, T2> ec, out MB<T1> buffer1,
            out MB<T2> buffer2, out ManagedEntityIDs entityIDs, out int count) where T1 : struct, IEntityViewComponent
            where T2 : struct, IEntityViewComponent
        {
            buffer1   = ec.buffer1._managedBuffer;
            buffer2   = ec.buffer2._managedBuffer;
            count     = (int)ec.count;
            entityIDs = ec.buffer1._managedIndices;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Deconstruct<T1, T2, T3>(in this EntityCollection<T1, T2, T3> ec, out MB<T1> buffer1,
            out MB<T2> buffer2, out MB<T3> buffer3, out int count) where T1 : struct, IEntityViewComponent
            where T2 : struct, IEntityViewComponent
            where T3 : struct, IEntityViewComponent
        {
            buffer1 = ec.buffer1._managedBuffer;
            buffer2 = ec.buffer2._managedBuffer;
            buffer3 = ec.buffer3._managedBuffer;
            count   = (int)ec.count;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Deconstruct<T1, T2, T3>(in this EntityCollection<T1, T2, T3> ec, out MB<T1> buffer1,
            out MB<T2> buffer2, out MB<T3> buffer3, out ManagedEntityIDs entityIDs, out int count)
            where T1 : struct, IEntityViewComponent
            where T2 : struct, IEntityViewComponent
            where T3 : struct, IEntityViewComponent
        {
            buffer1   = ec.buffer1._managedBuffer;
            buffer2   = ec.buffer2._managedBuffer;
            buffer3   = ec.buffer3._managedBuffer;
            count     = (int)ec.count;
            entityIDs = ec.buffer1._managedIndices;
        }
    }

    public static class EntityCollectionExtensionC
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Deconstruct<T1, T2>(in this EntityCollection<T1, T2> ec, out NB<T1> buffer1,
            out MB<T2> buffer2, out int count) where T1 : unmanaged, IEntityComponent
            where T2 : struct, IEntityViewComponent
        {
            buffer1 = ec.buffer1._nativedBuffer;
            buffer2 = ec.buffer2._managedBuffer;
            count   = (int)ec.count;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Deconstruct<T1, T2, T3>(in this EntityCollection<T1, T2, T3> ec, out NB<T1> buffer1,
            out MB<T2> buffer2, out MB<T3> buffer3, out int count) where T1 : unmanaged, IEntityComponent
            where T2 : struct, IEntityViewComponent
            where T3 : struct, IEntityViewComponent
        {
            buffer1 = ec.buffer1._nativedBuffer;
            buffer2 = ec.buffer2._managedBuffer;
            buffer3 = ec.buffer3._managedBuffer;
            count   = (int)ec.count;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Deconstruct<T1, T2, T3, T4>(in this EntityCollection<T1, T2, T3, T4> ec, out NB<T1> buffer1,
            out NB<T2> buffer2, out NB<T3> buffer3, out MB<T4> buffer4, out int count)
            where T1 : unmanaged, IEntityComponent
            where T2 : unmanaged, IEntityComponent
            where T3 : unmanaged, IEntityComponent
            where T4 : struct, IEntityViewComponent
        {
            buffer1 = ec.buffer1._nativedBuffer;
            buffer2 = ec.buffer2._nativedBuffer;
            buffer3 = ec.buffer3._nativedBuffer;
            buffer4 = ec.buffer4._managedBuffer;
            count   = (int)ec.count;
        }
    }

    public static class EntityCollectionExtensionD
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Deconstruct<T1, T2, T3>(in this EntityCollection<T1, T2, T3> ec, out NB<T1> buffer1,
            out NB<T2> buffer2, out MB<T3> buffer3, out int count) where T1 : unmanaged, IEntityComponent
            where T2 : unmanaged, IEntityComponent
            where T3 : struct, IEntityViewComponent
        {
            buffer1 = ec.buffer1._nativedBuffer;
            buffer2 = ec.buffer2._nativedBuffer;
            buffer3 = ec.buffer3._managedBuffer;
            count   = (int)ec.count;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Deconstruct<T1, T2, T3, T4>(in this EntityCollection<T1, T2, T3, T4> ec, out NB<T1> buffer1,
            out NB<T2> buffer2, out MB<T3> buffer3, out MB<T4> buffer4, out int count)
            where T1 : unmanaged, IEntityComponent
            where T2 : unmanaged, IEntityComponent
            where T3 : struct, IEntityViewComponent
            where T4 : struct, IEntityViewComponent
        {
            buffer1 = ec.buffer1._nativedBuffer;
            buffer2 = ec.buffer2._nativedBuffer;
            buffer3 = ec.buffer3._managedBuffer;
            buffer4 = ec.buffer4._managedBuffer;
            count   = (int)ec.count;
        }
    }
}