using System;
using System.Runtime.CompilerServices;
using Svelto.Common;
using Allocator = Svelto.Common.Allocator;

namespace Svelto.ECS.DataStructures
{
    public struct NativeDynamicArray : IDisposable
    {
#if UNITY_ECS        
        [global::Unity.Collections.LowLevel.Unsafe.NativeDisableUnsafePtrRestriction]
#endif
        unsafe UnsafeArray* _list;
#if DEBUG && !PROFILE_SVELTO
        int hashType;
#endif

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public uint Count<T>() where T:unmanaged
        {
            unsafe
            {
#if DEBUG && !PROFILE_SVELTO
                if (_list == null)
                    throw new Exception("NativeDynamicArray: null-access");
                if (hashType != TypeHash<T>.hash)
                    throw new Exception("NativeDynamicArray: not excepted type used");

#endif            
                return (uint) (_list->count / MemoryUtilities.SizeOf<T>());
            }
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public uint Capacity<T>() where T:unmanaged
        {
            unsafe
            {
#if DEBUG && !PROFILE_SVELTO
                if (_list == null)
                    throw new Exception("NativeDynamicArray: null-access");
                if (hashType != TypeHash<T>.hash)
                    throw new Exception("NativeDynamicArray: not excepted type used");

#endif            
                return (uint) (_list->capacity / MemoryUtilities.SizeOf<T>());
            }
        }

        public static NativeDynamicArray Alloc<T>(Allocator allocator, uint newLength = 0) where T : unmanaged
        {
            unsafe
            {
                var rtnStruc = new NativeDynamicArray();
#if DEBUG && !PROFILE_SVELTO
                rtnStruc.hashType = TypeHash<T>.hash;
#endif
                var sizeOf  = MemoryUtilities.SizeOf<T>();

                uint pointerSize = (uint) MemoryUtilities.SizeOf<UnsafeArray>();
                UnsafeArray* listData =
                    (UnsafeArray*) MemoryUtilities.Alloc<UnsafeArray>(pointerSize, allocator);
                
                //clear to nullify the pointers
                MemoryUtilities.MemClear((IntPtr) listData, pointerSize);

                listData->allocator = allocator;
                listData->Realloc<T>((uint) (newLength * sizeOf));

                rtnStruc._list = listData;

                return rtnStruc;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref T Get<T>(uint index) where T : unmanaged
        {
            unsafe
            {
#if DEBUG && !PROFILE_SVELTO
                if (_list == null)
                    throw new Exception("NativeDynamicArray: null-access");
                if (hashType != TypeHash<T>.hash)
                    throw new Exception("NativeDynamicArray: not excepted type used");
                if (index >= Count<T>())
                    throw new Exception($"NativeDynamicArray: out of bound access, index {index} count {Count<T>()}");
#endif
                return ref _list->Get<T>(index);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Set<T>(uint index, in T value) where T : unmanaged
        {
            unsafe
            {
#if DEBUG && !PROFILE_SVELTO
                if (_list == null)
                    throw new Exception("NativeDynamicArray: null-access");
                if (hashType != TypeHash<T>.hash)
                    throw new Exception("NativeDynamicArray: not excepted type used");
                if (index >= Capacity<T>())
                    throw new Exception($"NativeDynamicArray: out of bound access, index {index} count {Count<T>()}");
#endif            
                _list->Set(index, value);
            }
        }

        public unsafe void Dispose()
        {
#if DEBUG && !PROFILE_SVELTO        
            if (_list == null)
                throw new Exception("NativeDynamicArray: null-access");
#endif
                _list->Dispose();
                _list = null;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Add<T>(in T item) where T : unmanaged
        {
            unsafe
            {
#if DEBUG && !PROFILE_SVELTO
                if (_list == null)
                    throw new Exception("NativeDynamicArray: null-access");
                if (hashType != TypeHash<T>.hash)
                    throw new Exception("NativeDynamicArray: not excepted type used");
#endif
                var structSize = (uint) MemoryUtilities.SizeOf<T>();
                
                if (_list->space -  (int)structSize <  0)
                    _list->Realloc<T>((uint) ((Count<T>() + 1) * structSize * 1.5f));
           
                _list->Add(item);
            }
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AddWithoutGrow<T>(in T item) where T : unmanaged
        {
            unsafe
            {
#if DEBUG && !PROFILE_SVELTO
                if (_list == null)
                    throw new Exception("NativeDynamicArray: null-access");
                if (hashType != TypeHash<T>.hash)
                    throw new Exception("NativeDynamicArray: not excepted type used");

                var structSize = (uint) MemoryUtilities.SizeOf<T>();
                
                if (_list->space - (int)structSize < 0)
                    throw new Exception("NativeDynamicArray: no writing authorized");
#endif
                _list->Add(item);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Clear()
        {
            unsafe
            {
#if DEBUG && !PROFILE_SVELTO
                if (_list == null)
                    throw new Exception("NativeDynamicArray: null-access");
#endif
                _list->Clear();
            }
        }

        public unsafe T* ToPTR<T>() where T : unmanaged
        {
#if DEBUG && !PROFILE_SVELTO
            if (_list == null)
                throw new Exception("NativeDynamicArray: null-access");
            if (hashType != TypeHash<T>.hash)
                throw new Exception("NativeDynamicArray: not excepted type used");

#endif
            return (T*) _list->ptr;
        }

        public T[] ToManagedArray<T>() where T : unmanaged
        {
            unsafe
            {
#if DEBUG && !PROFILE_SVELTO
                if (_list == null)
                    throw new Exception("NativeDynamicArray: null-access");
                if (hashType != TypeHash<T>.hash)
                    throw new Exception("NativeDynamicArray: not excepted type used");

#endif
                var ret = new T[Count<T>()];

                fixed (void * handle = ret)
                {
                    Buffer.MemoryCopy(_list->ptr, handle, _list->count, _list->count);
                }

                return ret;
            }
        }
        
        public T[] ToManagedArrayUntrimmed<T>() where T : unmanaged
        {
            unsafe
            {
#if DEBUG && !PROFILE_SVELTO
                if (_list == null)
                    throw new Exception("NativeDynamicArray: null-access");
                if (hashType != TypeHash<T>.hash)
                    throw new Exception("NativeDynamicArray: not excepted type used");

#endif
                var ret = new T[Capacity<T>()];

                fixed (void * handle = ret)
                {
                    Buffer.MemoryCopy(_list->ptr, handle, _list->capacity, _list->capacity);
                }

                return ret;
            }
        }
    }
}
