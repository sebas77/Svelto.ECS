using System;
using System.Runtime.CompilerServices;
using Svelto.Common;

namespace Svelto.ECS.DataStructures
{
    //Necessary to be sure that the user won't pass random values
    public struct UnsafeArrayIndex
    {
        internal uint index;
    }

    /// <summary>
    ///     Note: this must work inside burst, so it must follow burst restrictions
    ///     It's a typeless native queue based on a ring-buffer model. This means that the writing head and the
    ///     reading head always advance independently. If there is enough space left by dequeued elements,
    ///     the writing head will wrap around if it reaches the end of the array. The writing head cannot ever surpass the reading head.
    ///  
    /// </summary>
    struct UnsafeBlob : IDisposable
    {
        internal unsafe byte* ptr { get; set; }

        //expressed in bytes
        internal uint capacity { get; private set; }

        //expressed in bytes
        internal uint size
        {
            get
            {
                var currentSize = (uint) _writeIndex - _readIndex;
#if DEBUG && !PROFILE_SVELTO
                if ((currentSize & (4 - 1)) != 0)
                    throw new Exception("size is expected to be a multiple of 4");
#endif

                return currentSize;
            }
        }

        //expressed in bytes
        internal uint availableSpace => capacity - size;

        /// <summary>
        /// </summary>
        internal Allocator allocator;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void Enqueue<T>(in T item) where T : struct
        {
            unsafe
            {
                var structSize = (uint) MemoryUtilities.SizeOf<T>();
                var writeHead  = _writeIndex % capacity;

#if DEBUG && !PROFILE_SVELTO
                var size  = _writeIndex - _readIndex;
                var spaceAvailable = capacity - size;
                if (spaceAvailable - (int) structSize < 0)
                    throw new Exception("no writing authorized");

                if ((writeHead & (4 - 1)) != 0)
                    throw new Exception("write head is expected to be a multiple of 4");
#endif
                if (writeHead + structSize <= capacity)
                {
                    Unsafe.Write(ptr + writeHead, item);
                }
                else //copy with wrap, will start to copy and wrap for the remainder
                {
                    var byteCountToEnd = capacity - writeHead;

                    var localCopyToAvoidGcIssues = item;
                    //read and copy the first portion of Item until the end of the stream
                    Unsafe.CopyBlock(ptr + writeHead, Unsafe.AsPointer(ref localCopyToAvoidGcIssues)
                                   , (uint) byteCountToEnd);

                    var restCount = structSize - byteCountToEnd;

                    //read and copy the remainder
                    Unsafe.CopyBlock(ptr, (byte*) Unsafe.AsPointer(ref localCopyToAvoidGcIssues) + byteCountToEnd
                                   , (uint) restCount);
                }

                //this is may seems a waste if you are going to use an unsafeBlob just for bytes, but it's necessary for mixed types.
                //it's still possible to use WriteUnaligned though
                uint paddedStructSize = (uint) (structSize + (int) MemoryUtilities.Pad4(structSize));

                _writeIndex += paddedStructSize; //we want _writeIndex to be always aligned by 4
            }
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        //The index returned is the index of the unwrapped ring. It must be wrapped again before to be used
        internal ref T Reserve<T>(out UnsafeArrayIndex index) where T : struct
        {
            unsafe
            {
                var structSize   = (uint) MemoryUtilities.SizeOf<T>();
                var wrappedIndex = _writeIndex % capacity;
#if DEBUG && !PROFILE_SVELTO
                var size           = _writeIndex - _readIndex;
                var spaceAvailable = capacity - size;
                if (spaceAvailable - (int) structSize < 0)
                    throw new Exception("no writing authorized");

                if ((wrappedIndex & (4 - 1)) != 0)
                    throw new Exception("write head is expected to be a multiple of 4");
#endif
                ref var buffer = ref Unsafe.AsRef<T>(ptr + wrappedIndex);

                index.index = _writeIndex;

                _writeIndex += structSize + MemoryUtilities.Pad4(structSize);

                return ref buffer;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal ref T AccessReserved<T>(UnsafeArrayIndex index) where T : struct
        {
            unsafe
            {
                var wrappedIndex = index.index % capacity;
#if DEBUG && !PROFILE_SVELTO
                if ((index.index & 3) != 0)
                    throw new Exception($"invalid index detected");
#endif
                return ref Unsafe.AsRef<T>(ptr + wrappedIndex);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal T Dequeue<T>() where T : struct
        {
            unsafe
            {
                var structSize = (uint) MemoryUtilities.SizeOf<T>();
                var readHead   = _readIndex % capacity;

#if DEBUG && !PROFILE_SVELTO
                var size = _writeIndex - _readIndex;
                if (size < structSize) //are there enough bytes to read?
                    throw new Exception("dequeuing empty queue or unexpected type dequeued");
                if (_readIndex > _writeIndex)
                    throw new Exception("unexpected read");
                if ((readHead & (4 - 1)) != 0)
                    throw new Exception("read head is expected to be a multiple of 4");
#endif
                var paddedStructSize = structSize + MemoryUtilities.Pad4(structSize);
                _readIndex += paddedStructSize;

                if (_readIndex == _writeIndex)
                {
                    //resetting the Indices has the benefit to let the Reserve work in more occasions and
                    //the rapping happening less often. If the _readIndex reached the _writeIndex, it means
                    //that there is no data left to read, so we can start to write again from the begin of the memory
                    _writeIndex = 0;
                    _readIndex  = 0;
                }

                if (readHead + paddedStructSize <= capacity)
                    return Unsafe.Read<T>(ptr + readHead);

                //handle the case the structure wraps around so it must be reconstructed from the part at the 
                //end of the stream and the part starting from the begin.
                T   item           = default;
                var byteCountToEnd = capacity - readHead;
                Unsafe.CopyBlock(Unsafe.AsPointer(ref item), ptr + readHead, byteCountToEnd);

                var restCount = structSize - byteCountToEnd;
                Unsafe.CopyBlock((byte*) Unsafe.AsPointer(ref item) + byteCountToEnd, ptr, restCount);

                return item;
            }
        }

        /// <summary>
        /// This version of Realloc unwraps a queue, but doesn't change the unwrapped index of existing elements.
        /// In this way the previously indices will remain valid
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void Realloc(uint newCapacity)
        {
            unsafe
            {
                //be sure it's multiple of 4. Assuming that what we write is aligned to 4, then we will always have aligned wrapped heads.
                //the reading and writing head always increment in multiple of 4
                newCapacity += MemoryUtilities.Pad4(newCapacity);

                byte* newPointer = null;
#if DEBUG && !PROFILE_SVELTO
                if (newCapacity <= capacity)
                    throw new Exception("new capacity must be bigger than current");
#endif
                newPointer = (byte*) MemoryUtilities.Alloc(newCapacity, allocator);

                //copy wrapped content if there is any
                var currentSize = _writeIndex - _readIndex;
                if (currentSize > 0)
                {
                    var oldReaderHead = _readIndex % capacity;
                    var writerHead = _writeIndex % capacity;

                    //there was no wrapping
                    if (oldReaderHead < writerHead)
                    {
                        var newReaderHead = _readIndex % newCapacity;
                        
                        Unsafe.CopyBlock(newPointer + newReaderHead, ptr + oldReaderHead, (uint) currentSize);
                    }
                    else
                    {
                        var byteCountToEnd = capacity - oldReaderHead;
                        var newReaderHead = _readIndex % newCapacity;
                        
#if DEBUG && !PROFILE_SVELTO
                        if (newReaderHead + byteCountToEnd + writerHead > newCapacity)
                            throw new Exception("something is wrong with my previous assumptions");
#endif                  
                        Unsafe.CopyBlock(newPointer + newReaderHead, ptr + oldReaderHead, byteCountToEnd); //from the old reader head to the end of the old array
                        Unsafe.CopyBlock(newPointer + newReaderHead + byteCountToEnd, ptr + 0, (uint) writerHead); //from the begin of the old array to the old writer head (rember the writerHead wrapped)
                    }
                }

                if (ptr != null)
                    MemoryUtilities.Free((IntPtr) ptr, allocator);

                ptr      = newPointer;
                capacity = newCapacity;

                //_readIndex  = 0; readIndex won't change to keep the previous reserved indices valid
                _writeIndex = _readIndex + currentSize;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Dispose()
        {
            unsafe
            {
                if (ptr != null)
                    MemoryUtilities.Free((IntPtr) ptr, allocator);

                ptr         = null;
                _writeIndex = 0;
                capacity    = 0;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Clear()
        {
            _writeIndex = 0;
            _readIndex  = 0;
        }

        uint _writeIndex;
        uint _readIndex;
    }
}
