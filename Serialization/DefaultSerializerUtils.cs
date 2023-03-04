using System;
using Svelto.ECS.Internal;

public static class DefaultSerializerUtils
{
    public static unsafe void CopyToByteArray<T>(in T src, byte[] data, uint offsetDst) where T : unmanaged, _IInternalEntityComponent
    {
#if DEBUG && !PROFILE_SVELTO
        if (data.Length - offsetDst < sizeof(T))
        {
            throw new IndexOutOfRangeException(
                $"Data out of bounds when copying struct {typeof(T).GetType().Name}. data.Length: {data.Length}, offsetDst: {offsetDst}");
        }
#endif

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

    public static unsafe T CopyFromByteArray<T>(byte[] data, uint offsetSrc) where T : unmanaged, _IInternalEntityComponent
    {
        T dst = default;

#if DEBUG && !PROFILE_SVELTO
        if (data.Length - offsetSrc < sizeof(T))
        {
            throw new IndexOutOfRangeException(
                $"Data out of bounds when copying struct {dst.GetType().Name}. data.Length: {data.Length}, offsetSrc: {offsetSrc}");
        }
#endif

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