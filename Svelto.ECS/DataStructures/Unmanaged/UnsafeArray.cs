using System;
using System.Runtime.CompilerServices;
using Svelto.Common;

namespace Svelto.ECS.DataStructures
{
    struct UnsafeArray
    {
        internal unsafe byte* ptr => _ptr;

        //expressed in bytes
        internal int capacity => (int) _capacity;

        //expressed in bytes
        internal int count => (int) _writeIndex;

        //expressed in bytes
        internal int space => capacity - count;

#if DEBUG && !PROFILE_SVELTO        
#pragma warning disable 649
        internal uint id;
#pragma warning restore 649
#endif        
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref T Get<T>(uint index) where T : struct
        {
            unsafe
            {
#if DEBUG && !PROFILE_SVELTO              
                uint sizeOf = (uint) MemoryUtilities.SizeOf<T>();
                if (index + sizeOf > _writeIndex)
                    throw new Exception("no reading authorized");
#endif                
                return ref Unsafe.AsRef<T>(Unsafe.Add<T>(ptr, (int) index));
            }
        }    
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Set<T>(uint index, in T value) where T : struct
        {
            unsafe
            {
                uint sizeOf = (uint) MemoryUtilities.SizeOf<T>();
                uint writeIndex = (uint) (index * sizeOf);
                
#if DEBUG && !PROFILE_SVELTO                
                if (_capacity < writeIndex + sizeOf)
                    throw new Exception("no writing authorized");
#endif
                Unsafe.AsRef<T>(Unsafe.Add<T>(_ptr, (int) index)) = value;

                if (_writeIndex <  writeIndex + sizeOf)
                    _writeIndex = (uint) (writeIndex + sizeOf);
            }
        }
        
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Add<T>(in T value) where T : struct
        {
            unsafe
            {
                var structSize = MemoryUtilities.SizeOf<T>();
                
#if DEBUG && !PROFILE_SVELTO                
                if (space - structSize < 0)
                    throw new Exception("no writing authorized");
#endif                
                Unsafe.Write(ptr + _writeIndex, value);

                _writeIndex += (uint)structSize;
            }
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref T Pop<T>() where T : struct
        {
            unsafe
            {
                var structSize = MemoryUtilities.SizeOf<T>();
                
                _writeIndex -= (uint)structSize;
            
                return ref Unsafe.AsRef<T>(ptr + _writeIndex);
            }
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void Realloc(uint newCapacity, Allocator allocator)
        {
            unsafe
            {
                if (_ptr == null)
                    _ptr = (byte*) MemoryUtilities.Alloc(newCapacity, allocator);
                else
                    _ptr = (byte*) MemoryUtilities.Realloc((IntPtr) _ptr, (uint) count, newCapacity, allocator);

                _capacity = newCapacity;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Dispose(Allocator allocator)
        {
            unsafe
            {
                if (ptr != null)
                    MemoryUtilities.Free((IntPtr) ptr, allocator);

                _ptr        = null;
                _writeIndex = 0;
                _capacity = 0;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Clear()
        {
            _writeIndex = 0;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetCountTo(uint count)
        {
            _writeIndex = count;
        }
        
#if UNITY_NATIVE
        [global::Unity.Collections.LowLevel.Unsafe.NativeDisableUnsafePtrRestriction]
#endif
        unsafe byte* _ptr;
        
        uint _writeIndex;
        uint _capacity;
    }
}