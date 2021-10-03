using System;
using System.Runtime.CompilerServices;
using Svelto.Common;
using Allocator = Svelto.Common.Allocator;

namespace Svelto.ECS.DataStructures
{
    public struct NativeDynamicArray : IDisposable
    {
        public bool isValid
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                unsafe
                {
                    return _list != null;
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int Count<T>() where T : struct
        {
            unsafe
            {
#if DEBUG && !PROFILE_SVELTO
                if (_list == null)
                    throw new Exception("NativeDynamicArray: null-access");
                if (_hashType != TypeHash<T>.hash)
                    throw new Exception($"NativeDynamicArray: not expected type used");

#endif
                return (_list->count / MemoryUtilities.SizeOf<T>());
            }
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int Size() 
        {
            unsafe
            {
#if DEBUG && !PROFILE_SVELTO
                if (_list == null)
                    throw new Exception("NativeDynamicArray: null-access");

#endif
                return (_list->count);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int Capacity<T>() where T : struct
        {
            unsafe
            {
#if DEBUG && !PROFILE_SVELTO
                if (_list == null)
                    throw new Exception("NativeDynamicArray: null-access");
                if (_hashType != TypeHash<T>.hash)
                    throw new Exception("NativeDynamicArray: not expected type used");

#endif
                return (_list->capacity / MemoryUtilities.SizeOf<T>());
            }
        }

        public static NativeDynamicArray Alloc<T>(uint newLength = 0) where T : struct
        {
            return Alloc<T>(Allocator.Persistent, newLength);
        }

        public static NativeDynamicArray Alloc<T>(Allocator allocator, uint newLength = 0) where T : struct
        {
            unsafe
            {
#if DEBUG && !PROFILE_SVELTO
                var rtnStruc = new NativeDynamicArray {_hashType = TypeHash<T>.hash};
#else           
                NativeDynamicArray rtnStruc = default;
#endif
                UnsafeArray* listData    = (UnsafeArray*) MemoryUtilities.Alloc<UnsafeArray>(1, allocator);

                //clear to nullify the pointers
                //MemoryUtilities.MemClear((IntPtr) listData, structSize);

                rtnStruc._allocator = allocator;
                listData->Realloc<T>(newLength, allocator);

                rtnStruc._list = listData;

                return rtnStruc;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref T Get<T>(uint index) where T : struct
        {
            unsafe
            {
#if DEBUG && !PROFILE_SVELTO
                if (_list == null)
                    throw new Exception("NativeDynamicArray: null-access");
                if (_hashType != TypeHash<T>.hash)
                    throw new Exception("NativeDynamicArray: not expected type used");
                if (index >= Count<T>())
                    throw new Exception($"NativeDynamicArray: out of bound access, index {index} count {Count<T>()}");
#endif
                return ref _list->Get<T>(index);
            }
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref T Get<T>(int index) where T : struct
        {
            return ref Get<T>((uint) index);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Set<T>(uint index, in T value) where T : struct
        {
            unsafe
            {
#if DEBUG && !PROFILE_SVELTO
                if (_list == null)
                    throw new Exception("NativeDynamicArray: null-access");
                if (_hashType != TypeHash<T>.hash)
                    throw new Exception("NativeDynamicArray: not expected type used");
                if (index >= Capacity<T>())
                    throw new Exception($"NativeDynamicArray: out of bound access, index {index} capacity {Capacity<T>()}");
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
            _list->Dispose(_allocator);
            MemoryUtilities.Free((IntPtr) _list, _allocator);
            
            _list = null;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Add<T>(in T item) where T : struct
        {
            unsafe
            {
#if DEBUG && !PROFILE_SVELTO
                if (_list == null)
                    throw new Exception("NativeDynamicArray: null-access");
                if (_hashType != TypeHash<T>.hash)
                    throw new Exception("NativeDynamicArray: not expected type used");
#endif
                if (Count<T>() == Capacity<T>())
                {
                    _list->Realloc<T>((uint) ((Capacity<T>() + 1) * 1.5f), _allocator);
                }

                _list->Add(item);
            }
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref T AddAt<T>(uint index) where T : struct
        {
            unsafe
            {
#if DEBUG && !PROFILE_SVELTO
                if (_list == null)
                    throw new Exception("NativeDynamicArray: null-access");
                if (_hashType != TypeHash<T>.hash)
                    throw new Exception("NativeDynamicArray: not expected type used");
#endif
                var structSize = (uint) MemoryUtilities.SizeOf<T>();

                if (index >= Capacity<T>())
                    _list->Realloc<T>((uint) ((index + 1) * 1.5f), _allocator);

                var writeIndex = (index + 1) * structSize;
                if (_list->count < writeIndex)
                    _list->SetCountTo(writeIndex);

                return ref _list->Get<T>(index);
            }
        }
        
        public void Resize<T>(uint newCapacity) where T : struct
        {
            unsafe
            {
#if DEBUG && !PROFILE_SVELTO
                if (_list == null)
                    throw new Exception("NativeDynamicArray: null-access");
                if (_hashType != TypeHash<T>.hash)
                    throw new Exception("NativeDynamicArray: not expected type used");
#endif
                _list->Realloc<T>((uint) newCapacity, _allocator);
            }
        }

        public void SetCount<T>(uint count) where T : struct
        {
            unsafe
            {
#if DEBUG && !PROFILE_SVELTO
                if (_list == null)
                    throw new Exception("NativeDynamicArray: null-access");
                if (_hashType != TypeHash<T>.hash)
                    throw new Exception("NativeDynamicArray: not expected type used");
#endif
                uint structSize = (uint) MemoryUtilities.SizeOf<T>();
                uint size       = (uint) (count * structSize);

                _list->SetCountTo((uint) size);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AddWithoutGrow<T>(in T item) where T : struct
        {
            unsafe
            {
#if DEBUG && !PROFILE_SVELTO
                if (_list == null)
                    throw new Exception("NativeDynamicArray: null-access");
                if (_hashType != TypeHash<T>.hash)
                    throw new Exception("NativeDynamicArray: not expected type used");

                var structSize = (uint) MemoryUtilities.SizeOf<T>();
                
                if (_list->space - (int)structSize < 0)
                    throw new Exception("NativeDynamicArray: no writing authorized");
#endif
                _list->Add(item);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void UnorderedRemoveAt<T>(uint index) where T : struct
        {
            unsafe
            {
#if DEBUG && !PROFILE_SVELTO
                if (_list == null)
                    throw new Exception("NativeDynamicArray: null-access");
                if (_hashType != TypeHash<T>.hash)
                    throw new Exception("NativeDynamicArray: not expected type used");
                if (Count<T>() == 0)
                    throw new Exception("NativeDynamicArray: empty array invalid operation");
#endif
                var indexToMove = Count<T>() - 1;
                if (index < indexToMove)
                {
                    Set<T>(index, Get<T>((uint) indexToMove));
                }

                _list->Pop<T>();
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void FastClear()
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
            if (_hashType != TypeHash<T>.hash)
                throw new Exception("NativeDynamicArray: not expected type used");

#endif
            return (T*) _list->ptr;
        }

        public IntPtr ToIntPTR<T>() where T : struct
        {
            unsafe
            {
#if DEBUG && !PROFILE_SVELTO
            if (_list == null)
                throw new Exception("NativeDynamicArray: null-access");
            if (_hashType != TypeHash<T>.hash)
                throw new Exception("NativeDynamicArray: not expected type used");

#endif
                return (IntPtr) _list->ptr;
            }
        }

        public T[] ToManagedArray<T>() where T : unmanaged
        {
            unsafe
            {
#if DEBUG && !PROFILE_SVELTO
                if (_list == null)
                    throw new Exception("NativeDynamicArray: null-access");
                if (_hashType != TypeHash<T>.hash)
                    throw new Exception("NativeDynamicArray: not expected type used");

#endif
                var count               = Count<T>();
                var ret                 = new T[count];
                var lengthToCopyInBytes = count * MemoryUtilities.SizeOf<T>();

                fixed (void* handle = ret)
                {
                    Unsafe.CopyBlock(handle, _list->ptr, (uint) lengthToCopyInBytes);
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
                if (_hashType != TypeHash<T>.hash)
                    throw new Exception("NativeDynamicArray: not expected type used");
#endif
                var capacity            = Capacity<T>();
                var lengthToCopyInBytes = capacity * MemoryUtilities.SizeOf<T>();
                var ret                 = new T[capacity];

                fixed (void* handle = ret)
                {
                    Unsafe.CopyBlock(handle, _list->ptr, (uint) lengthToCopyInBytes);
                }

                return ret;
            }
        }

        public void RemoveAt<T>(uint index) where T : struct
        {
            unsafe
            {
#if DEBUG && !PROFILE_SVELTO
                if (_list == null)
                    throw new Exception("NativeDynamicArray: null-access");
                if (_hashType != TypeHash<T>.hash)
                    throw new Exception("NativeDynamicArray: not expected type used");
#endif

                var sizeOf = MemoryUtilities.SizeOf<T>();
                //Unsafe.CopyBlock may not be memory overlapping safe (memcpy vs memmove)
                Buffer.MemoryCopy(_list->ptr + (index + 1) * sizeOf, _list->ptr + index * sizeOf, _list->count
                                , (uint) ((Count<T>() - (index + 1)) * sizeOf));
                _list->Pop<T>();
            }
        }

        public void MemClear()
        {
            unsafe
            {
                MemoryUtilities.MemClear((IntPtr) _list->ptr, (uint) _list->capacity);
            }
        }
        
#if UNITY_COLLECTIONS || UNITY_JOBS || UNITY_BURST
        [global::Unity.Burst.NoAlias] [global::Unity.Collections.LowLevel.Unsafe.NativeDisableUnsafePtrRestriction]
#endif
        unsafe UnsafeArray* _list;
#if DEBUG && !PROFILE_SVELTO
        int _hashType;
#endif
        Allocator _allocator;
    }
}