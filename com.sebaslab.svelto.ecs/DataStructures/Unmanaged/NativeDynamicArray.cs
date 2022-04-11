#if DEBUG && !PROFILE_SVELTO
#define ENABLE_DEBUG_CHECKS
#endif

using System;
using System.Runtime.CompilerServices;
using Svelto.Common;
using Svelto.Common.DataStructures;
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
#if ENABLE_DEBUG_CHECKS
                if (_list == null)
                    throw new Exception("NativeDynamicArray: null-access");
                if (_hashType != TypeHash<T>.hash)
                    throw new Exception($"NativeDynamicArray: not expected type used");

#endif
                return (_list->count / MemoryUtilities.SizeOf<T>());
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int SizeInBytes()
        {
            unsafe
            {
#if ENABLE_DEBUG_CHECKS
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
#if ENABLE_DEBUG_CHECKS
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
#if ENABLE_DEBUG_CHECKS
                var rtnStruc = new NativeDynamicArray
                {
                    _hashType       = TypeHash<T>.hash,
                };
#else
                NativeDynamicArray rtnStruc = default;
#endif
                UnsafeArray* listData = (UnsafeArray*)MemoryUtilities.Alloc<UnsafeArray>(1, allocator);

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
#if ENABLE_DEBUG_CHECKS
                if (_list == null)
                    throw new Exception("NativeDynamicArray: null-access");
                if (_hashType != TypeHash<T>.hash)
                    throw new Exception("NativeDynamicArray: not expected type used");
                if (index >= Count<T>())
                    throw new Exception($"NativeDynamicArray: out of bound access, index {index} count {Count<T>()}");
#endif
                
#if ENABLE_DEBUG_CHECKS
                using (_threadSentinel.TestThreadSafety())
                {
#endif
                    return ref _list->Get<T>(index);
#if ENABLE_DEBUG_CHECKS                    
                }
#endif
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref T Get<T>(int index) where T : struct
        {
            return ref Get<T>((uint)index);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Set<T>(uint index, in T value) where T : struct
        {
            unsafe
            {
#if ENABLE_DEBUG_CHECKS
                if (_list == null)
                    throw new Exception("NativeDynamicArray: null-access");
                if (_hashType != TypeHash<T>.hash)
                    throw new Exception("NativeDynamicArray: not expected type used");
                if (index >= Capacity<T>())
                    throw new Exception(
                        $"NativeDynamicArray: out of bound access, index {index} capacity {Capacity<T>()}");
#endif
                
#if ENABLE_DEBUG_CHECKS
                using (_threadSentinel.TestThreadSafety())
                {
#endif
                    _list->Set(index, value);
                    
#if ENABLE_DEBUG_CHECKS
                }
#endif
            }
        }

        public unsafe void Dispose()
        {
#if ENABLE_DEBUG_CHECKS
            if (_list == null)
                throw new Exception("NativeDynamicArray: null-access");
#endif
            
#if ENABLE_DEBUG_CHECKS
            using (_threadSentinel.TestThreadSafety())
            {
#endif
                _list->Dispose(_allocator);
                MemoryUtilities.Free((IntPtr)_list, _allocator);
                
#if ENABLE_DEBUG_CHECKS
            }
#endif
            _list = null;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Add<T>(in T item) where T : struct
        {
            unsafe
            {
#if ENABLE_DEBUG_CHECKS
                if (_list == null)
                    throw new Exception("NativeDynamicArray: null-access");
                if (_hashType != TypeHash<T>.hash)
                    throw new Exception("NativeDynamicArray: not expected type used");
#endif
                
#if ENABLE_DEBUG_CHECKS
                using (_threadSentinel.TestThreadSafety())
                {
#endif
                    if (Count<T>() == Capacity<T>())
                    {
                        _list->Realloc<T>((uint)((Capacity<T>() + 1) * 1.5f), _allocator);
                    }

                    _list->Add(item);
#if ENABLE_DEBUG_CHECKS                    
                }
#endif
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref T AddAt<T>(uint index) where T : struct
        {
            unsafe
            {
#if ENABLE_DEBUG_CHECKS
                if (_list == null)
                    throw new Exception("NativeDynamicArray: null-access");
                if (_hashType != TypeHash<T>.hash)
                    throw new Exception("NativeDynamicArray: not expected type used");
#endif
                var structSize = (uint)MemoryUtilities.SizeOf<T>();

#if ENABLE_DEBUG_CHECKS
                using (_threadSentinel.TestThreadSafety())
                {
#endif
                    if (index >= Capacity<T>())
                        _list->Realloc<T>((uint)((index + 1) * 1.5f), _allocator);

                    var writeIndex = (index + 1) * structSize;
                    if (_list->count < writeIndex)
                        _list->SetCountTo(writeIndex);

                    return ref _list->Get<T>(index);
#if ENABLE_DEBUG_CHECKS                    
                }
#endif
            }
        }

        public void Resize<T>(uint newCapacity) where T : struct
        {
            unsafe
            {
#if ENABLE_DEBUG_CHECKS
                if (_list == null)
                    throw new Exception("NativeDynamicArray: null-access");
                if (_hashType != TypeHash<T>.hash)
                    throw new Exception("NativeDynamicArray: not expected type used");
#endif
                
#if ENABLE_DEBUG_CHECKS
                using (_threadSentinel.TestThreadSafety())
                {
#endif
                    _list->Realloc<T>((uint)newCapacity, _allocator);
                    
#if ENABLE_DEBUG_CHECKS
                }
#endif
            }
        }

        public void SetCount<T>(uint count) where T : struct
        {
            unsafe
            {
#if ENABLE_DEBUG_CHECKS
                if (_list == null)
                    throw new Exception("NativeDynamicArray: null-access");
                if (_hashType != TypeHash<T>.hash)
                    throw new Exception("NativeDynamicArray: not expected type used");
#endif
                uint structSize = (uint)MemoryUtilities.SizeOf<T>();
                uint size       = (uint)(count * structSize);

#if ENABLE_DEBUG_CHECKS
                using (_threadSentinel.TestThreadSafety())
                {
#endif
                    _list->SetCountTo((uint)size);
                    
#if ENABLE_DEBUG_CHECKS
                }
#endif
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AddWithoutGrow<T>(in T item) where T : struct
        {
            unsafe
            {
#if ENABLE_DEBUG_CHECKS
                if (_list == null)
                    throw new Exception("NativeDynamicArray: null-access");
                if (_hashType != TypeHash<T>.hash)
                    throw new Exception("NativeDynamicArray: not expected type used");

                var structSize = (uint)MemoryUtilities.SizeOf<T>();

                if (_list->space - (int)structSize < 0)
                    throw new Exception("NativeDynamicArray: no writing authorized");
#endif
                
#if ENABLE_DEBUG_CHECKS
                using (_threadSentinel.TestThreadSafety())
                {
#endif
                    _list->Add(item);
                    
#if ENABLE_DEBUG_CHECKS
                }
#endif
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void UnorderedRemoveAt<T>(uint index) where T : struct
        {
            unsafe
            {
#if ENABLE_DEBUG_CHECKS
                if (_list == null)
                    throw new Exception("NativeDynamicArray: null-access");
                if (_hashType != TypeHash<T>.hash)
                    throw new Exception("NativeDynamicArray: not expected type used");
                if (Count<T>() == 0)
                    throw new Exception("NativeDynamicArray: empty array invalid operation");
#endif

#if ENABLE_DEBUG_CHECKS
                using (_threadSentinel.TestThreadSafety())
                {
#endif
                    var indexToMove = Count<T>() - 1;
                    if (index < indexToMove)
                    {
                        Set<T>(index, Get<T>((uint)indexToMove));
                    }

                    _list->Pop<T>();
                    
#if ENABLE_DEBUG_CHECKS
                }
#endif
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void FastClear()
        {
            unsafe
            {
#if ENABLE_DEBUG_CHECKS
                if (_list == null)
                    throw new Exception("NativeDynamicArray: null-access");
#endif
                
#if ENABLE_DEBUG_CHECKS
                using (_threadSentinel.TestThreadSafety())
                {
#endif
                    _list->Clear();
                    
#if ENABLE_DEBUG_CHECKS
                }
#endif
            }
        }

        public unsafe T* ToPTR<T>() where T : unmanaged
        {
#if ENABLE_DEBUG_CHECKS
            if (_list == null)
                throw new Exception("NativeDynamicArray: null-access");
            if (_hashType != TypeHash<T>.hash)
                throw new Exception("NativeDynamicArray: not expected type used");

#endif
            return (T*)_list->ptr;
        }

        public IntPtr ToIntPTR<T>() where T : struct
        {
            unsafe
            {
#if ENABLE_DEBUG_CHECKS
                if (_list == null)
                    throw new Exception("NativeDynamicArray: null-access");
                if (_hashType != TypeHash<T>.hash)
                    throw new Exception("NativeDynamicArray: not expected type used");

#endif
                return (IntPtr)_list->ptr;
            }
        }

        public T[] ToManagedArray<T>() where T : unmanaged
        {
            unsafe
            {
#if ENABLE_DEBUG_CHECKS
                if (_list == null)
                    throw new Exception("NativeDynamicArray: null-access");
                if (_hashType != TypeHash<T>.hash)
                    throw new Exception("NativeDynamicArray: not expected type used");

#endif
                var count               = Count<T>();
                var ret                 = new T[count];
                var lengthToCopyInBytes = count * MemoryUtilities.SizeOf<T>();

#if ENABLE_DEBUG_CHECKS
                using (_threadSentinel.TestThreadSafety())
                {
#endif
                    fixed (void* handle = ret)
                    {
                        Unsafe.CopyBlock(handle, _list->ptr, (uint)lengthToCopyInBytes);
                    }

                    return ret;
                    
#if ENABLE_DEBUG_CHECKS
                }
#endif
            }
        }

        public T[] ToManagedArrayUntrimmed<T>() where T : unmanaged
        {
            unsafe
            {
#if ENABLE_DEBUG_CHECKS
                if (_list == null)
                    throw new Exception("NativeDynamicArray: null-access");
                if (_hashType != TypeHash<T>.hash)
                    throw new Exception("NativeDynamicArray: not expected type used");
#endif
                var capacity = Capacity<T>();
                var ret      = new T[capacity];

#if ENABLE_DEBUG_CHECKS
                using (_threadSentinel.TestThreadSafety())
                {
#endif
                    fixed (void* handle = ret)
                    {
                        MemoryUtilities.MemCpy<T>((IntPtr)_list->ptr, 0, (IntPtr)handle, 0, (uint)capacity);
                    }
                    
#if ENABLE_DEBUG_CHECKS
                }
#endif

                return ret;
            }
        }

        public void RemoveAt<T>(uint index) where T : struct
        {
            unsafe
            {
#if ENABLE_DEBUG_CHECKS
                if (_list == null)
                    throw new Exception("NativeDynamicArray: null-access");
                if (_hashType != TypeHash<T>.hash)
                    throw new Exception("NativeDynamicArray: not expected type used");
#endif

#if ENABLE_DEBUG_CHECKS
                using (_threadSentinel.TestThreadSafety())
                {
#endif
                    MemoryUtilities.MemMove<T>((IntPtr)_list->ptr, index + 1, index, (uint)(Count<T>() - (index + 1)));

                    _list->Pop<T>();
                    
#if ENABLE_DEBUG_CHECKS
                }
#endif
            }
        }

        public void MemClear()
        {
            unsafe
            {
#if ENABLE_DEBUG_CHECKS
                using (_threadSentinel.TestThreadSafety())
                {
#endif
                    MemoryUtilities.MemClear((IntPtr)_list->ptr, (uint)_list->capacity);
#if ENABLE_DEBUG_CHECKS
                }
#endif
            }
        }

#if UNITY_COLLECTIONS || UNITY_JOBS || UNITY_BURST
#if UNITY_BURST
        [Unity.Burst.NoAlias]
#endif
        [Unity.Collections.LowLevel.Unsafe.NativeDisableUnsafePtrRestriction]
#endif
        unsafe UnsafeArray* _list;
#if DEBUG && !PROFILE_SVELTO
        int _hashType;
#endif
        
        Sentinel _threadSentinel;

        Allocator _allocator;
    }
}