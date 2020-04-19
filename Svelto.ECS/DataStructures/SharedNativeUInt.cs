using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;

namespace Svelto.ECS.DataStructures
{
    public struct SharedNativeUInt: IDisposable
    {
#if UNITY_ECS        
        [global::Unity.Collections.LowLevel.Unsafe.NativeDisableUnsafePtrRestriction]
#endif
        unsafe uint* data;

        public SharedNativeUInt(uint t)
        {
            unsafe
            {
                data  = (uint*) Marshal.AllocHGlobal(sizeof(uint));
                *data = t;
            }
        }
        
        public static implicit operator uint(SharedNativeUInt t)
        {
            unsafe
            {
#if DEBUG && !PROFILE_SVELTO                
                if (t.data == null)               
                    throw new Exception("using disposed SharedUInt");
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
                int result = Interlocked.Decrement(ref Unsafe.As<uint, int>(ref *data));
                
#if DEBUG && !PROFILE_SVELTO                
                if (result < 0)               
                    throw new Exception("can't have negative numbers");
#endif                
            }
        }
        
        public void Increment()
        {
            unsafe
            {
                Interlocked.Increment(ref Unsafe.As<uint, int>(ref *data));
            }
        }
    }
}