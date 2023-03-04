using System;
using Svelto.Common;
using Svelto.DataStructures;
using Svelto.ECS.Internal;

namespace Svelto.ECS
{
    public struct Consumer<T> : IDisposable where T : unmanaged, _IInternalEntityComponent
    {
        internal Consumer(string name, uint capacity) : this()
        {
            unsafe
            {
#if DEBUG && !PROFILE_SVELTO
                _name = name;
#endif
                _ringBuffer = new RingBuffer<ValueTuple<T, EGID>>((int) capacity,
#if DEBUG && !PROFILE_SVELTO
                                                                  _name
#else
                string.Empty
#endif
                );
                mustBeDisposed          = MemoryUtilities.NativeAlloc<bool>(1, Allocator.Persistent);
                *(bool*) mustBeDisposed = false;

                isActive          = MemoryUtilities.NativeAlloc<bool>(1, Allocator.Persistent);
                *(bool*) isActive = true;
            }
        }

        internal Consumer(ExclusiveGroupStruct group, string name, uint capacity) : this(name, capacity)
        {
            this.group = group;
            hasGroup   = true;
        }

        internal void Enqueue(in T entity, in EGID egid)
        {
            unsafe
            {
                if (*(bool*)isActive)
                    _ringBuffer.Enqueue((entity, egid));
            }
        }

        public bool TryDequeue(out T entity)
        {
            var tryDequeue = _ringBuffer.TryDequeue(out var values);

            entity = values.Item1;

            return tryDequeue;
        }

        //Note: it is correct to publish the EGID at the moment of the publishing, as the responsibility of 
        //the publisher consumer is not tracking the real state of the entity in the database at the 
        //moment of the consumption, but it's instead to store a copy of the entity at the moment of the publishing
        public bool TryDequeue(out T entity, out EGID id)
        {
            var tryDequeue = _ringBuffer.TryDequeue(out var values);

            entity = values.Item1;
            id     = values.Item2;

            return tryDequeue;
        }

        public void Flush() { _ringBuffer.Reset(); }

        public void Dispose()
        {
            unsafe
            {
                *(bool*) mustBeDisposed = true;
            }
        }

        public uint Count() { return (uint) _ringBuffer.Count; }

        public void Free()
        {
            MemoryUtilities.NativeFree(mustBeDisposed, Allocator.Persistent);
            MemoryUtilities.NativeFree(isActive,       Allocator.Persistent);
        }

        public void Pause()
        {
            unsafe
            {
                *(bool*) isActive = false;
            }
        }

        public void Resume()
        {
            unsafe
            {
                *(bool*) isActive = true;
            }
        }

        readonly RingBuffer<ValueTuple<T, EGID>> _ringBuffer;

        internal readonly ExclusiveGroupStruct group;
        internal readonly bool                 hasGroup;
        internal          IntPtr               isActive;
        internal          IntPtr               mustBeDisposed;

#if DEBUG && !PROFILE_SVELTO
        readonly string _name;
#endif
    }
}