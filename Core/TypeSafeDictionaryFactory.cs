using Svelto.Common;
using Svelto.ECS.Hybrid;
using Svelto.ECS.Internal;

namespace Svelto.ECS
{
    static class TypeSafeDictionaryFactory<T> where T : struct, _IInternalEntityComponent
    {
        static readonly bool isUnmanaged = TypeCache<T>.isUnmanaged
                                        && typeof(IEntityViewComponent).IsAssignableFrom(typeof(T)) == false;

        public static ITypeSafeDictionary Create(uint size)
        {
            if (isUnmanaged)
                return new UnmanagedTypeSafeDictionary<T>(size);
            
            return new ManagedTypeSafeDictionary<T>(size);
        }
    }
}