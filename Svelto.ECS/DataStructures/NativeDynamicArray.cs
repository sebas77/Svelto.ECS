using System;
using System.Runtime.CompilerServices;
using Svelto.Common;
using Allocator = Svelto.Common.Allocator;

namespace Svelto.ECS.DataStructures
{
    public struct NativeDynamicArray : IDisposable
    {
#if UNITY_COLLECTIONS
        [global::Unity.Burst.NoAlias] [global::Unity.Collections.LowLevel.Unsafe.NativeDisableUnsafePtrRestriction]
#endif
        unsafe UnsafeArray* _list;
#if DEBUG && !PROFILE_SVELTO
        int hashType;
#endif

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int Count<T>() where T : struct
        {
            unsafe
            {
#if DEBUG && !PROFILE_SVELTO
                if (_list == null)
                    throw new Exception("NativeDynamicArray: null-access");
                if (hashType != TypeHash<T>.hash)
                    throw new Exception("NativeDynamicArray: not excepted type used");

#endif
                return (_list->count / MemoryUtilities.SizeOf<T>());
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
                if (hashType != TypeHash<T>.hash)
                    throw new Exception("NativeDynamicArray: not excepted type used");

#endif
                return (_list->capacity / MemoryUtilities.SizeOf<T>());
            }
        }

        public static NativeDynamicArray Alloc<T>(Allocator allocator, uint newLength = 0) where T : struct
        {
            unsafe
            {
                var rtnStruc = new NativeDynamicArray();
#if DEBUG && !PROFILE_SVELTO
                rtnStruc.hashType = TypeHash<T>.hash;
#endif
                var sizeOf = MemoryUtilities.SizeOf<T>();

                uint         pointerSize = (uint) MemoryUtilities.SizeOf<UnsafeArray>();
                UnsafeArray* listData    = (UnsafeArray*) MemoryUtilities.Alloc(pointerSize, allocator);

                //clear to nullify the pointers
                MemoryUtilities.MemClear((IntPtr) listData, pointerSize);

                listData->allocator = allocator;
                listData->Realloc((uint) (newLength * sizeOf));

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
                if (hashType != TypeHash<T>.hash)
                    throw new Exception("NativeDynamicArray: not excepted type used");
                if (index >= Count<T>())
                    throw new Exception($"NativeDynamicArray: out of bound access, index {index} count {Count<T>()}");
#endif
                return ref _list->Get<T>(index);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Set<T>(uint index, in T value) where T : struct
        {
            unsafe
            {
#if DEBUG && !PROFILE_SVELTO
                if (_list == null)
                    throw new Exception("NativeDynamicArray: null-access");
                if (hashType != TypeHash<T>.hash)
                    throw new Exception("NativeDynamicArray: not excepted type used");
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
            _list->Dispose();
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
                if (hashType != TypeHash<T>.hash)
                    throw new Exception("NativeDynamicArray: not excepted type used");
#endif
                var structSize = (uint) MemoryUtilities.SizeOf<T>();

                if (_list->space - (int) structSize < 0)
                    _list->Realloc((uint) (((uint) ((Count<T>() + 1) * 1.5f) * (float) structSize)));

                _list->Add(item);
            }
        }

        public void Grow<T>(uint newCapacity) where T : struct
        {
            unsafe
            {
#if DEBUG && !PROFILE_SVELTO
                if (_list == null)
                    throw new Exception("NativeDynamicArray: null-access");
                if (hashType != TypeHash<T>.hash)
                    throw new Exception("NativeDynamicArray: not excepted type used");
                if (newCapacity <= Capacity<T>())
                    throw new Exception("New capacity must be greater than current one");
#endif
                uint structSize = (uint) MemoryUtilities.SizeOf<T>();

                uint size = (uint) (newCapacity * structSize);
                _list->Realloc((uint) size);
            }
        }

        public void SetCount<T>(uint count) where T : struct
        {
            unsafe
            {
#if DEBUG && !PROFILE_SVELTO
                if (_list == null)
                    throw new Exception("NativeDynamicArray: null-access");
                if (hashType != TypeHash<T>.hash)
                    throw new Exception("NativeDynamicArray: not excepted type used");
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
        public void UnorderedRemoveAt<T>(uint index) where T : struct
        {
            unsafe
            {
#if DEBUG && !PROFILE_SVELTO
                if (_list == null)
                    throw new Exception("NativeDynamicArray: null-access");
                if (hashType != TypeHash<T>.hash)
                    throw new Exception("NativeDynamicArray: not excepted type used");
                if (Count<T>() == 0)
                    throw new Exception("NativeDynamicArray: empty array invalid operation");
#endif
                var count = Count<T>() - 1;
                if (index < count)
                {
                    Set<T>(index, Get<T>((uint) count));
                }

                _list->Pop<T>();
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

        public IntPtr ToIntPTR<T>() where T : struct
        {
            unsafe
            {
#if DEBUG && !PROFILE_SVELTO
            if (_list == null)
                throw new Exception("NativeDynamicArray: null-access");
            if (hashType != TypeHash<T>.hash)
                throw new Exception("NativeDynamicArray: not excepted type used");

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
                if (hashType != TypeHash<T>.hash)
                    throw new Exception("NativeDynamicArray: not excepted type used");

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
                if (hashType != TypeHash<T>.hash)
                    throw new Exception("NativeDynamicArray: not excepted type used");
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
                if (hashType != TypeHash<T>.hash)
                    throw new Exception("NativeDynamicArray: not excepted type used");
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
    }

    public ref struct NativeDynamicArrayCast<T> where T : struct
    {
        NativeDynamicArray _array;

        public NativeDynamicArrayCast(NativeDynamicArray array) : this() { _array = array; }

        public int count
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _array.Count<T>();
        }

        public ref T this[int index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => ref _array.Get<T>((uint) index);
        }

        public ref T this[uint index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => ref _array.Get<T>(index);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Add(in T id) { _array.Add(id); }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void UnorderedRemoveAt(uint index) { _array.UnorderedRemoveAt<T>(index); }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void RemoveAt(uint index) { _array.RemoveAt<T>(index); }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Clear() { _array.Clear(); }
    }
}