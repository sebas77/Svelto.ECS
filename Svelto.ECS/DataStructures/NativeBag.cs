using System;
using System.Runtime.CompilerServices;
using Svelto.Common;

namespace Svelto.ECS.DataStructures
{
    /// <summary>
    ///     Burst friendly RingBuffer on steroid:
    ///     it can: Enqueue/Dequeue, it wraps if there is enough space after dequeuing
    ///     It resizes if there isn't enough space left.
    ///     It's a "bag", you can queue and dequeue any T. Just be sure that you dequeue what you queue! No check on type
    ///     is done.
    ///     You can reserve a position in the queue to update it later.
    ///     The datastructure is a struct and it's "copyable"
    ///     I eventually decided to call it NativeBag and not NativeBag because it can also be used as
    ///     a preallocated memory pool where any kind of T can be stored as long as T is unmanaged
    /// </summary>
    public struct NativeBag : IDisposable
    {
#if UNITY_COLLECTIONS
        [global::Unity.Collections.LowLevel.Unsafe.NativeDisableUnsafePtrRestriction]
#endif
        unsafe UnsafeBlob* _queue;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsEmpty()
        {
            unsafe
            {
                if (_queue == null || _queue->ptr == null)
                    return true;
            }

            return count == 0;
        }

        public uint count
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                unsafe
                {
#if DEBUG && !PROFILE_SVELTO
                    if (_queue == null)
                        throw new Exception("SimpleNativeArray: null-access");
#endif

                    return _queue->size;
                }
            }
        }

        public uint capacity
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                unsafe
                {
#if DEBUG && !PROFILE_SVELTO
                    if (_queue == null)
                        throw new Exception("SimpleNativeArray: null-access");
#endif

                    return _queue->capacity;
                }
            }
        }

        public NativeBag(Allocator allocator)
        {
            unsafe
            {
                var sizeOf = MemoryUtilities.SizeOf<UnsafeBlob>();
                var listData = (UnsafeBlob*) MemoryUtilities.Alloc((uint) sizeOf, allocator);

                //clear to nullify the pointers
                MemoryUtilities.MemClear((IntPtr) listData, (uint) sizeOf);
                listData->allocator = allocator;
                _queue = listData;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe void Dispose()
        {
            if (_queue != null)
            {
                _queue->Dispose();
                _queue = null;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref T ReserveEnqueue<T>(out UnsafeArrayIndex index) where T : struct
        {
            unsafe
            {
#if DEBUG && !PROFILE_SVELTO
                if (_queue == null)
                    throw new Exception("SimpleNativeArray: null-access");
#endif
                var sizeOf = MemoryUtilities.SizeOf<T>();
                if (_queue->space - sizeOf < 0)
                    _queue->Realloc((uint) ((_queue->capacity + sizeOf) * 2.0f));

                return ref _queue->Reserve<T>(out index);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Enqueue<T>(in T item) where T : struct
        {
            unsafe
            {
#if DEBUG && !PROFILE_SVELTO
                if (_queue == null)
                    throw new Exception("SimpleNativeArray: null-access");
#endif
                var sizeOf = MemoryUtilities.SizeOf<T>();
                if (_queue->space - sizeOf < 0)
                    _queue->Realloc((uint) ((_queue->capacity + MemoryUtilities.Align4((uint) sizeOf)) * 2.0f));

                _queue->Write(item);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Clear()
        {
            unsafe
            {
#if DEBUG && !PROFILE_SVELTO
                if (_queue == null)
                    throw new Exception("SimpleNativeArray: null-access");
#endif
                _queue->Clear();
            }
        }

        public T Dequeue<T>() where T : struct
        {
            unsafe
            {
                return _queue->Read<T>();
            }
        }

        public ref T AccessReserved<T>(UnsafeArrayIndex reserverIndex) where T : struct
        {
            unsafe
            {
#if DEBUG && !PROFILE_SVELTO
                if (_queue == null)
                    throw new Exception("SimpleNativeArray: null-access");
#endif
                return ref _queue->AccessReserved<T>(reserverIndex);
            }
        }
    }
}