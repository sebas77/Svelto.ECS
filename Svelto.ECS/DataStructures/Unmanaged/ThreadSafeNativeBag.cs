#if later
using System;
using System.Runtime.CompilerServices;
using System.Threading;
using Svelto.Common;
using Svelto.Utilities;

namespace Svelto.ECS.DataStructures
{
    /// <summary>
    ///     Burst friendly Ring Buffer on steroid:
    ///     it can: Enqueue/Dequeue, it wraps if there is enough space after dequeuing
    ///     It resizes if there isn't enough space left.
    ///     It's a "bag", you can queue and dequeue any T. Just be sure that you dequeue what you queue! No check on type
    ///     is done.
    ///     You can reserve a position in the queue to update it later.
    ///     The datastructure is a struct and it's "copyable"
    ///     I eventually decided to call it NativeBag and not NativeBag because it can also be used as
    ///     a preallocated memory pool where any kind of T can be stored as long as T is unmanaged
    /// </summary>
    public struct ThreadSafeNativeBag : IDisposable
    {
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

        public ThreadSafeNativeBag(Allocator allocator)
        {
            unsafe
            {
                var sizeOf = MemoryUtilities.SizeOf<UnsafeBlob>();
                var listData = (UnsafeBlob*) MemoryUtilities.Alloc((uint) sizeOf, allocator);

                //clear to nullify the pointers
                //MemoryUtilities.MemClear((IntPtr) listData, (uint) sizeOf);
                listData->allocator = allocator;
                _queue = listData;
            }

            _writingGuard = 0;
        }
        
        public ThreadSafeNativeBag(Allocator allocator, uint capacity)
        {
            unsafe
            {
                var sizeOf   = MemoryUtilities.SizeOf<UnsafeBlob>();
                var listData = (UnsafeBlob*) MemoryUtilities.Alloc((uint) sizeOf, allocator);

                //clear to nullify the pointers
                //MemoryUtilities.MemClear((IntPtr) listData, (uint) sizeOf);
                listData->allocator = allocator;
                _queue              = listData;
                _queue->Realloc(capacity);
            }

            _writingGuard = 0;
        }

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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe void Dispose()
        {
            if (_queue != null)
            {
                _queue->Dispose();
                MemoryUtilities.Free((IntPtr) _queue, _queue->allocator);
                _queue = null;
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
                var sizeOf      = MemoryUtilities.SizeOf<T>();
                var alignedSize = (uint) MemoryUtilities.SizeOfAligned<T>();

                Interlocked.MemoryBarrier();
                Reset:
                var oldCapacity         = _queue->capacity;
                var spaceleft = oldCapacity - (_queue->_writeIndex - _queue->_readIndex) - sizeOf;

                while (spaceleft < 0)
                {
                    //if _writingGuard is not equal to 0, it means that another thread increased the
                    //value so it's possible the reallocing is already happening OR it means that 
                    //writing are still in progress and we must be sure that are all flushed first
                    if (Interlocked.CompareExchange(ref _writingGuard, 1, 0) != 0)
                    {
                        ThreadUtility.Yield();
                        goto Reset;
                    }
                    
                    var newCapacity = (uint) ((oldCapacity + alignedSize) * 2.0f);
                    Svelto.Console.Log($"realloc {newCapacity}");
                    _queue->Realloc(newCapacity);
                    
                    Volatile.Write(ref _writingGuard, 0);
                }
                
                int writeIndex;
                
                //look for the first available slot to write in
                writeIndex = _queue->_writeIndex;
                if (Interlocked.CompareExchange(ref _queue->_writeIndex, (int) (writeIndex + alignedSize)
                  , writeIndex) != writeIndex)
                {
                    ThreadUtility.Yield();
                    goto Reset;
                }

                 Interlocked.Increment(ref _writingGuard);
                _queue->Write(item, (uint) writeIndex);
                Interlocked.Decrement(ref _writingGuard);
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
        
#if UNITY_NATIVE
        [global::Unity.Collections.LowLevel.Unsafe.NativeDisableUnsafePtrRestriction]
#endif
        unsafe UnsafeBlob* _queue;

        int _writingGuard;
    }
}
#endif