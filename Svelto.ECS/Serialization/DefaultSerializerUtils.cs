using System;
using Svelto.ECS;

public static class DefaultSerializerUtils
{
    public static unsafe void CopyToByteArray<T>(in T src, byte[] data, uint offsetDst) where T : unmanaged, IEntityStruct
    {
        fixed (void* dstPtr = data)
        {
            void* dstOffsetPtr;
            if (IntPtr.Size == sizeof(int))
            {
                dstOffsetPtr = (void*) (((IntPtr) dstPtr).ToInt32() + ((IntPtr) offsetDst).ToInt32());
            }
            else
            {
                dstOffsetPtr = (void*) (((IntPtr) dstPtr).ToInt64() + ((IntPtr) offsetDst).ToInt64());
            }

            *(T*) dstOffsetPtr = src;
        }
    }

    public static unsafe T CopyFromByteArray<T>(byte[] data, uint offsetSrc) where T : unmanaged, IEntityStruct
    {
        T dst = new T();

        void* dstPtr = &dst;
        fixed (void* srcPtr = data)
        {
            void* srcOffsetPtr;
            if (IntPtr.Size == sizeof(int))
            {
                srcOffsetPtr = (void*) (((IntPtr) srcPtr).ToInt32() + ((IntPtr) offsetSrc).ToInt32());
            }
            else
            {
                srcOffsetPtr = (void*) (((IntPtr) srcPtr).ToInt64() + ((IntPtr) offsetSrc).ToInt64());
            }

            *(T*) dstPtr = *(T*) srcOffsetPtr;
        }

        return dst;
    }
}