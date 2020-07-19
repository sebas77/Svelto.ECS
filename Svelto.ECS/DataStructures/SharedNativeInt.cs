using System;
using System.Runtime.InteropServices;
using System.Threading;

namespace Svelto.ECS.DataStructures
{
    public struct SharedNativeInt: IDisposable
    {
#if UNITY_COLLECTIONS 
        [global::Unity.Collections.LowLevel.Unsafe.NativeDisableUnsafePtrRestriction]
#endif
        unsafe int* data;

        public static SharedNativeInt Create(int t)
        {
            unsafe
            {
                var current = new SharedNativeInt();
                current.data  = (int*) Marshal.AllocHGlobal(sizeof(int));
                *current.data = t;

                return current;
            }
        }
        
        public static implicit operator int(SharedNativeInt t)
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
#if DEBUG && !PROFILE_SVELTO
                if (data == null)
                    throw new Exception("disposing already disposed data");
#endif
                Marshal.FreeHGlobal((IntPtr) data);
                data = null;
            }
        }

        public int Decrement()
        {
            unsafe
            {
#if DEBUG && !PROFILE_SVELTO
                if (data == null)
                    throw new Exception("null-access");
#endif            
                
                return Interlocked.Decrement(ref *data);
            }
        }
        
        public int Increment()
        {
            unsafe
            {
#if DEBUG && !PROFILE_SVELTO
                if (data == null)
                    throw new Exception("null-access");
#endif            
                
                return Interlocked.Increment(ref *data);
            }
        }
    }
}