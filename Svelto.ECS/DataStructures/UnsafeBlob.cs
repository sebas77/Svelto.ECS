using System;
using System.Runtime.CompilerServices;
using Svelto.Common;
using Unity.Collections.LowLevel.Unsafe;

namespace Svelto.ECS.DataStructures
{
    public struct UnsafeArrayIndex
    {
        internal uint index;
        internal uint capacity;
    }

    /// <summary>
    /// Note: this must work inside burst, so it must follow burst restrictions
    /// Note: All the svelto native structures 
    /// </summary>
    struct UnsafeBlob : IDisposable
    {
        internal unsafe byte* ptr => _ptr;
        //expressed in bytes
        internal uint capacity { get; private set; }
        //expressed in bytes
        internal uint size => _writeIndex - _readIndex;
        //expressed in bytes
        internal uint space => capacity - size;

        /// <summary>
        /// </summary>
        internal Allocator allocator;
#if DEBUG && !PROFILE_SVELTO        
        internal uint id;
#endif        
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void Write<T>(in T item) where T : unmanaged
        {
            unsafe
            {
                var structSize = (uint) MemoryUtilities.SizeOf<T>();
            
                //the idea is, considering the wrap, a read pointer must always be behind a writer pointer
#if DEBUG && !PROFILE_SVELTO                
                if (space - (int)structSize < 0)
                    throw new Exception("no writing authorized");
#endif
                var writeHead = _writeIndex % capacity;

                if (writeHead + structSize <= capacity)
                {
                    Unsafe.Write(_ptr + writeHead, item);
                }
                else
                //copy with wrap, will start to copy and wrap for the reminder
                {
                    var byteCountToEnd = capacity - writeHead;

                    fixed (T* readFrom = &item)
                    {
                        //read and copy the first portion of Item until the end of the stream
                        Unsafe.CopyBlock(_ptr + writeHead, readFrom, byteCountToEnd);

                        var restCount = structSize - byteCountToEnd;

                        //read and copy the remainder
                        var @from = (byte*) readFrom;
                        Unsafe.CopyBlock(_ptr, @from + byteCountToEnd, restCount);
                    }
                }

                uint paddedStructSize = Align4(structSize);
                
                _writeIndex += paddedStructSize;
            }
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void WriteUnaligned<T>(in T item) where T : unmanaged
        {
            unsafe
            {
                uint structSize = (uint) MemoryUtilities.SizeOf<T>();
            
                //the idea is, considering the wrap, a read pointer must always be behind a writer pointer
#if DEBUG && !PROFILE_SVELTO                
                if (space - (int)structSize < 0)
                    throw new Exception("no writing authorized");
#endif
                var pointer = _writeIndex % capacity;

                if (pointer + structSize <= capacity)
                    Unsafe.Write(_ptr + pointer, item);
                else
                {
                    var byteCount = capacity - pointer;
                    fixed (T* readFrom = &item)
                    {
                        Unsafe.CopyBlockUnaligned(_ptr + pointer, readFrom, byteCount);

                        var    restCount = structSize - byteCount;
                        var @from = (byte*) readFrom;
                        Unsafe.CopyBlockUnaligned(_ptr, @from + byteCount, restCount);
                    }
                }

                _writeIndex += structSize;
            }
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal T Read<T>() where T : unmanaged
        {
            unsafe
            {
                var structSize = (uint) MemoryUtilities.SizeOf<T>();
                
#if DEBUG && !PROFILE_SVELTO            
                if (size < structSize) //are there enough bytes to read?
                    throw new Exception("dequeuing empty queue or unexpected type dequeued");
                if (_readIndex > _writeIndex)
                    throw new Exception("unexpected read");
#endif
                var head = _readIndex % capacity;
                uint paddedStructSize = Align4(structSize);
                _readIndex += paddedStructSize;

                if (_readIndex == _writeIndex)
                {
                    //resetting the Indices has the benefit to let the Reserve work in more occasions and
                    //the rapping happening less often. If the _readIndex reached the _writeIndex, it means
                    //that there is no data left to read, so we can start to write again from the begin of the memory
                    _writeIndex = 0;
                    _readIndex  = 0;
                }

                if (head + paddedStructSize <= capacity)
                {
                    return Unsafe.Read<T>(_ptr + head);
                }

                T   item           = default;
                T* destination = &item;
                var byteCountToEnd = capacity - head;
                Unsafe.CopyBlock(destination, _ptr + head, byteCountToEnd);

                var restCount = structSize - byteCountToEnd;
                Unsafe.CopyBlock((byte*) destination + byteCountToEnd, ptr, restCount);

                return item;
            }
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal ref T Reserve<T>(out UnsafeArrayIndex index) where T : unmanaged
        {
            unsafe
            {
                var sizeOf = (uint) MemoryUtilities.SizeOf<T>();
                
                T*  buffer = (T *)(_ptr + _writeIndex);
#if DEBUG && !PROFILE_SVELTO
                if (_writeIndex > capacity) throw new Exception($"can't reserve if the writeIndex wrapped around the capacity, writeIndex {_writeIndex} capacity {capacity}");
                if (_writeIndex + sizeOf > capacity) throw new Exception("out of bound reserving");
#endif                
                index = new UnsafeArrayIndex
                {
                    capacity = capacity
                  , index    = _writeIndex
                };

                var align4 = Align4(sizeOf);
                _writeIndex += align4;
            
                return ref buffer[0];
            }
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal ref T AccessReserved<T>(UnsafeArrayIndex index) where T : unmanaged
        {
            unsafe
            {
#if DEBUG && !PROFILE_SVELTO
                var size = MemoryUtilities.SizeOf<T>();
                if (index.index + size > capacity) throw new Exception($"out of bound access, index {index.index} size {size} capacity {capacity}");
#endif                
                T* buffer = (T*) (_ptr + index.index);
            
                return ref buffer[0];
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void Realloc<T>(uint newCapacity) where T : unmanaged
        {
            unsafe
            {
                //be sure it's multiple of 4. Assuming that what we write is aligned to 4, then we will always have aligned wrapped heads
                newCapacity = Align4(newCapacity); 
                
                byte* newPointer = null;
#if DEBUG && !PROFILE_SVELTO            
                if (newCapacity <= capacity)
                    throw new Exception("new capacity must be bigger than current");
#endif                
                if (newCapacity > 0)
                {
                    newPointer = (byte*) MemoryUtilities.Alloc<T>(newCapacity, allocator);
                    if (size > 0)
                    {
                        var readerHead = _readIndex % capacity;
                        var writerHead = _writeIndex % capacity;

                        if (readerHead < writerHead)
                        {
                            //copy to the new pointer, from th reader position
                            uint currentSize = _writeIndex - _readIndex;
                            Unsafe.CopyBlock(newPointer, _ptr + readerHead, currentSize);
                        }
                        //the assumption is that if size > 0 (so readerPointer and writerPointer are not the same)
                        //writerHead wrapped and reached readerHead. so I have to copy from readerHead to the end
                        //and from the start to writerHead (which is the same position of readerHead)
                        else
                        {
                            var byteCountToEnd = capacity - readerHead;

                            Unsafe.CopyBlock(newPointer, _ptr + readerHead, byteCountToEnd);
                            Unsafe.CopyBlock(newPointer + byteCountToEnd, _ptr, writerHead);
                        }
                    }
                }

                if (_ptr != null)
                    MemoryUtilities.Free((IntPtr) _ptr, allocator);

                _writeIndex = size;
                _readIndex  = 0;
            
                _ptr = newPointer;
                capacity = newCapacity;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Dispose()
        {
            unsafe
            {
                if (_ptr != null)
                    MemoryUtilities.Free((IntPtr) _ptr, allocator);

                _ptr = null;
                _writeIndex = 0;
                capacity     = 0;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Clear()
        {
            _writeIndex = 0;
            _readIndex = 0;
        }

        uint Align4(uint input)
        {
            return (uint)(Math.Ceiling(input / 4.0) * 4);            
        }
        
#if UNITY_ECS
        [NativeDisableUnsafePtrRestriction]
#endif
        unsafe byte* _ptr;
        uint _writeIndex, _readIndex;
    }
}
