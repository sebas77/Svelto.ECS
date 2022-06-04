using Svelto.Common;
using Svelto.ECS.Hybrid;
using Svelto.ECS.Internal;

namespace Svelto.ECS
{
    static class TypeSafeDictionaryFactory<T> where T : struct, IBaseEntityComponent
    {
        static readonly bool isUnmanaged = typeof(T).IsUnmanagedEx()
                                        && typeof(IEntityViewComponent).IsAssignableFrom(typeof(T)) == false;

        public static ITypeSafeDictionary Create()
        {
            if (isUnmanaged)
                return new UnmanagedTypeSafeDictionary<T>(1);
            
            return new ManagedTypeSafeDictionary<T>(1);
        }

        public static ITypeSafeDictionary Create(uint size)
        {
            if (isUnmanaged)
                return new UnmanagedTypeSafeDictionary<T>(size);
            
            return new ManagedTypeSafeDictionary<T>(size);
        }
    }
}