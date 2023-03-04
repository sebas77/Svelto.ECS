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
            if (ec._buffer == null) //I cannot test 0, as buffer can be valid and processed by removeEx with count 0 
            {
                buffer = default;
                count = 0;
                return;
            }

            buffer = (NB<T1>)ec._buffer;
            count = (int)ec.count;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Deconstruct<T1>(in this EntityCollection<T1> ec, out NB<T1> buffer,
            out NativeEntityIDs entityIDs, out int count)
                where T1 : unmanaged, IEntityComponent
        {
            if (ec._buffer == null) //I cannot test 0, as buffer can be valid and processed by removeEx with count 0
            {
                buffer = default;
                count = 0;
                entityIDs = default;
                return;
            }

            buffer = (NB<T1>)ec._buffer;
            count = (int)ec.count;
            entityIDs = (NativeEntityIDs)ec._entityIDs;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Deconstruct<T1, T2>(in this EntityCollection<T1, T2> ec, out NB<T1> buffer1,
            out NB<T2> buffer2, out NativeEntityIDs entityIDs, out int count)
                where T1 : unmanaged, IEntityComponent
                where T2 : unmanaged, IEntityComponent
        {
            if (ec.buffer1._buffer
             == null) //I cannot test 0, as buffer can be valid and processed by removeEx with count 0
            {
                buffer1 = default;
                buffer2 = default;
                count = 0;
                entityIDs = default;
                return;
            }

            buffer1 = (NB<T1>)ec.buffer1._buffer;
            buffer2 = (NB<T2>)ec.buffer2._buffer;
            count = ec.count;
            entityIDs = (NativeEntityIDs)ec.buffer1._entityIDs;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Deconstruct<T1, T2>(in this EntityCollection<T1, T2> ec, out NB<T1> buffer1,
            out NB<T2> buffer2, out int count)
                where T1 : unmanaged, IEntityComponent
                where T2 : unmanaged, IEntityComponent
        {
            if (ec.buffer1._buffer
             == null) //I cannot test 0, as buffer can be valid and processed by removeEx with count 0
            {
                buffer1 = default;
                buffer2 = default;
                count = 0;
                return;
            }

            buffer1 = (NB<T1>)ec.buffer1._buffer;
            buffer2 = (NB<T2>)ec.buffer2._buffer;
            count = (int)ec.count;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Deconstruct<T1, T2, T3>(in this EntityCollection<T1, T2, T3> ec, out NB<T1> buffer1,
            out NB<T2> buffer2, out NB<T3> buffer3, out int count)
                where T1 : unmanaged, IEntityComponent
                where T2 : unmanaged, IEntityComponent
                where T3 : unmanaged, IEntityComponent
        {
            if (ec.buffer1._buffer
             == null) //I cannot test 0, as buffer can be valid and processed by removeEx with count 0
            {
                buffer1 = default;
                buffer2 = default;
                buffer3 = default;
                count = 0;
                return;
            }

            buffer1 = (NB<T1>)ec.buffer1._buffer;
            buffer2 = (NB<T2>)ec.buffer2._buffer;
            buffer3 = (NB<T3>)ec.buffer3._buffer;
            count = (int)ec.count;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Deconstruct<T1, T2, T3>(in this EntityCollection<T1, T2, T3> ec, out NB<T1> buffer1,
            out NB<T2> buffer2, out NB<T3> buffer3, out NativeEntityIDs entityIDs, out int count)
                where T1 : unmanaged, IEntityComponent
                where T2 : unmanaged, IEntityComponent
                where T3 : unmanaged, IEntityComponent
        {
            if (ec.buffer1._buffer
             == null) //I cannot test 0, as buffer can be valid and processed by removeEx with count 0
            {
                buffer1 = default;
                buffer2 = default;
                buffer3 = default;
                count = 0;
                entityIDs = default;
                return;
            }

            buffer1 = (NB<T1>)ec.buffer1._buffer;
            buffer2 = (NB<T2>)ec.buffer2._buffer;
            buffer3 = (NB<T3>)ec.buffer3._buffer;
            count = (int)ec.count;
            entityIDs = (NativeEntityIDs)ec.buffer1._entityIDs;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Deconstruct<T1, T2, T3, T4>(in this EntityCollection<T1, T2, T3, T4> ec, out NB<T1> buffer1,
            out NB<T2> buffer2, out NB<T3> buffer3, out NB<T4> buffer4, out int count)
                where T1 : unmanaged, IEntityComponent
                where T2 : unmanaged, IEntityComponent
                where T3 : unmanaged, IEntityComponent
                where T4 : unmanaged, IEntityComponent
        {
            if (ec.buffer1._buffer
             == null) //I cannot test 0, as buffer can be valid and processed by removeEx with count 0
            {
                buffer1 = default;
                buffer2 = default;
                buffer3 = default;
                buffer4 = default;
                count = 0;
                return;
            }

            buffer1 = (NB<T1>)ec.buffer1._buffer;
            buffer2 = (NB<T2>)ec.buffer2._buffer;
            buffer3 = (NB<T3>)ec.buffer3._buffer;
            buffer4 = (NB<T4>)ec.buffer4._buffer;
            count = (int)ec.count;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Deconstruct<T1, T2, T3, T4>(in this EntityCollection<T1, T2, T3, T4> ec, out NB<T1> buffer1,
            out NB<T2> buffer2, out NB<T3> buffer3, out NB<T4> buffer4, out NativeEntityIDs entityIDs, out int count)
                where T1 : unmanaged, IEntityComponent
                where T2 : unmanaged, IEntityComponent
                where T3 : unmanaged, IEntityComponent
                where T4 : unmanaged, IEntityComponent
        {
            if (ec.buffer1._buffer
             == null) //I cannot test 0, as buffer can be valid and processed by removeEx with count 0
            {
                buffer1 = default;
                buffer2 = default;
                buffer3 = default;
                buffer4 = default;
                count = 0;
                entityIDs = default;
                return;
            }

            buffer1 = (NB<T1>)ec.buffer1._buffer;
            buffer2 = (NB<T2>)ec.buffer2._buffer;
            buffer3 = (NB<T3>)ec.buffer3._buffer;
            buffer4 = (NB<T4>)ec.buffer4._buffer;
            entityIDs = (NativeEntityIDs)ec.buffer1._entityIDs;
            count = (int)ec.count;
        }
    }

    public static class EntityCollectionExtensionB
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Deconstruct<T1>(in this EntityCollection<T1> ec, out MB<T1> buffer, out int count)
                where T1 : struct, IEntityViewComponent
        {
            if (ec._buffer == null) //I cannot test 0, as buffer can be valid and processed by removeEx with count 0
            {
                buffer = default;
                count = 0;
                return;
            }

            buffer = (MB<T1>)ec._buffer;
            count = (int)ec.count;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Deconstruct<T1>(in this EntityCollection<T1> ec, out MB<T1> buffer,
            out ManagedEntityIDs entityIDs, out int count)
                where T1 : struct, IEntityViewComponent
        {
            if (ec._buffer == null) //I cannot test 0, as buffer can be valid and processed by removeEx with count 0
            {
                buffer = default;
                count = 0;
                entityIDs = default;
                return;
            }

            buffer = (MB<T1>)ec._buffer;
            count = (int)ec.count;
            entityIDs = (ManagedEntityIDs)ec._entityIDs;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Deconstruct<T1, T2>(in this EntityCollection<T1, T2> ec, out MB<T1> buffer1,
            out MB<T2> buffer2, out int count)
                where T1 : struct, IEntityViewComponent
                where T2 : struct, IEntityViewComponent
        {
            if (ec.buffer1._buffer
             == null) //I cannot test 0, as buffer can be valid and processed by removeEx with count 0
            {
                buffer1 = default;
                buffer2 = default;
                count = 0;
                return;
            }

            buffer1 = (MB<T1>)ec.buffer1._buffer;
            buffer2 = (MB<T2>)ec.buffer2._buffer;
            count = (int)ec.count;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Deconstruct<T1, T2>(in this EntityCollection<T1, T2> ec, out MB<T1> buffer1,
            out MB<T2> buffer2, out ManagedEntityIDs entityIDs, out int count)
                where T1 : struct, IEntityViewComponent
                where T2 : struct, IEntityViewComponent
        {
            if (ec.buffer1._buffer
             == null) //I cannot test 0, as buffer can be valid and processed by removeEx with count 0
            {
                buffer1 = default;
                buffer2 = default;
                count = 0;
                entityIDs = default;
                return;
            }

            buffer1 = (MB<T1>)ec.buffer1._buffer;
            buffer2 = (MB<T2>)ec.buffer2._buffer;
            count = (int)ec.count;
            entityIDs = (ManagedEntityIDs)ec.buffer1._entityIDs;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Deconstruct<T1, T2, T3>(in this EntityCollection<T1, T2, T3> ec, out MB<T1> buffer1,
            out MB<T2> buffer2, out MB<T3> buffer3, out int count)
                where T1 : struct, IEntityViewComponent
                where T2 : struct, IEntityViewComponent
                where T3 : struct, IEntityViewComponent
        {
            if (ec.buffer1._buffer
             == null) //I cannot test 0, as buffer can be valid and processed by removeEx with count 0
            {
                buffer1 = default;
                buffer2 = default;
                buffer3 = default;
                count = 0;
                return;
            }

            buffer1 = (MB<T1>)ec.buffer1._buffer;
            buffer2 = (MB<T2>)ec.buffer2._buffer;
            buffer3 = (MB<T3>)ec.buffer3._buffer;
            count = (int)ec.count;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Deconstruct<T1, T2, T3>(in this EntityCollection<T1, T2, T3> ec, out MB<T1> buffer1,
            out MB<T2> buffer2, out MB<T3> buffer3, out ManagedEntityIDs entityIDs, out int count)
                where T1 : struct, IEntityViewComponent
                where T2 : struct, IEntityViewComponent
                where T3 : struct, IEntityViewComponent
        {
            if (ec.buffer1._buffer
             == null) //I cannot test 0, as buffer can be valid and processed by removeEx with count 0
            {
                buffer1 = default;
                buffer2 = default;
                buffer3 = default;
                entityIDs = default;
                count = 0;
                return;
            }

            buffer1 = (MB<T1>)ec.buffer1._buffer;
            buffer2 = (MB<T2>)ec.buffer2._buffer;
            buffer3 = (MB<T3>)ec.buffer3._buffer;
            count = (int)ec.count;
            entityIDs = (ManagedEntityIDs)ec.buffer1._entityIDs;
        }
    }

    public static class EntityCollectionExtensionC
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Deconstruct<T1, T2>(in this EntityCollection<T1, T2> ec, out NB<T1> buffer1,
            out MB<T2> buffer2, out ManagedEntityIDs entityIDs, out int count)
                where T1 : unmanaged, IEntityComponent
                where T2 : struct, IEntityViewComponent
        {
            if (ec.buffer1._buffer
             == null) //I cannot test 0, as buffer can be valid and processed by removeEx with count 0
            {
                buffer1 = default;
                buffer2 = default;
                count = 0;
                return;
            }

            buffer1 = (NB<T1>)ec.buffer1._buffer;
            buffer2 = (MB<T2>)ec.buffer2._buffer;
            count = (int)ec.count;
            entityIDs = (ManagedEntityIDs)ec.buffer2._entityIDs;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Deconstruct<T1, T2>(in this EntityCollection<T1, T2> ec, out NB<T1> buffer1,
            out MB<T2> buffer2, out int count)
                where T1 : unmanaged, IEntityComponent
                where T2 : struct, IEntityViewComponent
        {
            if (ec.buffer1._buffer
             == null) //I cannot test 0, as buffer can be valid and processed by removeEx with count 0
            {
                buffer1 = default;
                buffer2 = default;
                count = 0;
                return;
            }

            buffer1 = (NB<T1>)ec.buffer1._buffer;
            buffer2 = (MB<T2>)ec.buffer2._buffer;
            count = (int)ec.count;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Deconstruct<T1, T2, T3>(in this EntityCollection<T1, T2, T3> ec, out NB<T1> buffer1,
            out MB<T2> buffer2, out MB<T3> buffer3, out int count)
                where T1 : unmanaged, IEntityComponent
                where T2 : struct, IEntityViewComponent
                where T3 : struct, IEntityViewComponent
        {
            if (ec.buffer1._buffer
             == null) //I cannot test 0, as buffer can be valid and processed by removeEx with count 0
            {
                buffer1 = default;
                buffer2 = default;
                buffer3 = default;
                count = 0;
                return;
            }

            buffer1 = (NB<T1>)ec.buffer1._buffer;
            buffer2 = (MB<T2>)ec.buffer2._buffer;
            buffer3 = (MB<T3>)ec.buffer3._buffer;
            count = (int)ec.count;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Deconstruct<T1, T2, T3, T4>(in this EntityCollection<T1, T2, T3, T4> ec, out NB<T1> buffer1,
            out NB<T2> buffer2, out NB<T3> buffer3, out MB<T4> buffer4, out int count)
                where T1 : unmanaged, IEntityComponent
                where T2 : unmanaged, IEntityComponent
                where T3 : unmanaged, IEntityComponent
                where T4 : struct, IEntityViewComponent
        {
            if (ec.buffer1._buffer
             == null) //I cannot test 0, as buffer can be valid and processed by removeEx with count 0
            {
                buffer1 = default;
                buffer2 = default;
                buffer3 = default;
                buffer4 = default;
                count = 0;
                return;
            }

            buffer1 = (NB<T1>)ec.buffer1._buffer;
            buffer2 = (NB<T2>)ec.buffer2._buffer;
            buffer3 = (NB<T3>)ec.buffer3._buffer;
            buffer4 = (MB<T4>)ec.buffer4._buffer;
            count = (int)ec.count;
        }
    }

    public static class EntityCollectionExtensionD
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Deconstruct<T1, T2, T3>(in this EntityCollection<T1, T2, T3> ec, out NB<T1> buffer1,
            out NB<T2> buffer2, out MB<T3> buffer3, out int count)
                where T1 : unmanaged, IEntityComponent
                where T2 : unmanaged, IEntityComponent
                where T3 : struct, IEntityViewComponent
        {
            if (ec.buffer1._buffer
             == null) //I cannot test 0, as buffer can be valid and processed by removeEx with count 0
            {
                buffer1 = default;
                buffer2 = default;
                buffer3 = default;
                count = 0;
                return;
            }

            buffer1 = (NB<T1>)ec.buffer1._buffer;
            buffer2 = (NB<T2>)ec.buffer2._buffer;
            buffer3 = (MB<T3>)ec.buffer3._buffer;
            count = (int)ec.count;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Deconstruct<T1, T2, T3, T4>(in this EntityCollection<T1, T2, T3, T4> ec, out NB<T1> buffer1,
            out NB<T2> buffer2, out MB<T3> buffer3, out MB<T4> buffer4, out int count)
                where T1 : unmanaged, IEntityComponent
                where T2 : unmanaged, IEntityComponent
                where T3 : struct, IEntityViewComponent
                where T4 : struct, IEntityViewComponent
        {
            if (ec.buffer1._buffer
             == null) //I cannot test 0, as buffer can be valid and processed by removeEx with count 0
            {
                buffer1 = default;
                buffer2 = default;
                buffer3 = default;
                buffer4 = default;
                count = 0;
                return;
            }

            buffer1 = (NB<T1>)ec.buffer1._buffer;
            buffer2 = (NB<T2>)ec.buffer2._buffer;
            buffer3 = (MB<T3>)ec.buffer3._buffer;
            buffer4 = (MB<T4>)ec.buffer4._buffer;
            count = (int)ec.count;
        }
    }
}