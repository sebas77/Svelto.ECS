using System;
using System.Runtime.CompilerServices;
using Svelto.Common;

namespace Svelto.ECS.DataStructures
{
    public struct SharedDisposableNative<T> : IDisposable where T : unmanaged, IDisposable
    {
#if UNITY_COLLECTIONS || (UNITY_JOBS || UNITY_BURST)
        [global::Unity.Collections.LowLevel.Unsafe.NativeDisableUnsafePtrRestriction]
#endif        
        unsafe IntPtr ptr;

        public SharedDisposableNative(in T value)
        {
            unsafe
            {
                ptr = MemoryUtilities.Alloc<T>(1, Allocator.Persistent);
                Unsafe.Write((void*)ptr, value);
            }
        }

        public void Dispose()
        {
            unsafe
            {
                Unsafe.AsRef<T>((void*)ptr).Dispose();
                
                MemoryUtilities.Free((IntPtr)ptr, Allocator.Persistent);
                ptr = IntPtr.Zero;
            }
        }

        public ref T value
        {
            get
            {
                unsafe
                {
                    DBC.ECS.Check.Require(ptr != null, "SharedNative has not been initialized");

                    return ref Unsafe.AsRef<T>((void*)ptr);
                }
            }
        }
    }
}