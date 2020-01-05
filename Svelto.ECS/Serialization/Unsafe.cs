using Svelto.ECS;

namespace Svelto.Common
{
    public class UnsafeUtils
    {
        public static uint SizeOf<T>() where T : unmanaged, IEntityStruct
        {
            unsafe
            {
                return (uint) sizeof(T);
            }
        }
    }
}