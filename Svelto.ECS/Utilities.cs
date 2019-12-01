using System.Runtime.InteropServices;

namespace Svelto.ECS
{
    public static class Utilities
    {
        public static UnsafeStructRef<T> ToUnsafeRef<T>(this T[] entityStructs, uint index) where T : struct, IEntityStruct
        {
            var alloc = GCHandle.Alloc(entityStructs, GCHandleType.Pinned);
            
            return new UnsafeStructRef<T>(ref entityStructs[index], alloc);
        }
    }
}