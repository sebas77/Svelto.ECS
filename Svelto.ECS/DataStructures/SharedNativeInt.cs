using System;
using System.Runtime.InteropServices;
using System.Threading;

namespace Svelto.ECS.DataStructures
{
    public struct SharedNativeInt: IDisposable
    {
#if UNITY_ECS        
        [global::Unity.Collections.LowLevel.Unsafe.NativeDisableUnsafePtrRestriction]
#endif
        unsafe int* data;

        public static implicit operator SharedNativeInt(int t)
        {
            unsafe
            {
                var current = new SharedNativeInt();
                current.data  = (int*) Marshal.AllocHGlobal(sizeof(int));
                *current.data = t;

                return current;
            }
        }
        
        public static explicit operator int(SharedNativeInt t)
        {
            unsafe
            {
#if DEBUG && !PROFILE_SVELTO                
                if (t.data == null)               
                    throw new Exception("using disposed SharedInt");
#endif    

                return *t.data;
            }
        }

        public void Dispose()
        {
            unsafe
            {
                if (data != null)
                {
                    Marshal.FreeHGlobal((IntPtr) data);
                    data = null;
                }
            }
        }

        public void Decrement()
        {
            unsafe
            {
                Interlocked.Decrement(ref *data);
            }
        }
        
        public void Increment()
        {
            unsafe
            {
                Interlocked.Increment(ref *data);
            }
        }
    }
}