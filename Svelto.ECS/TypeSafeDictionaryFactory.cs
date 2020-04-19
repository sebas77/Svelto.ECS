using Svelto.ECS.Internal;

namespace Svelto.ECS
{
    static class TypeSafeDictionaryFactory<T> where T : struct, IEntityComponent
    {
        public static ITypeSafeDictionary Create()
        {
            return new TypeSafeDictionary<T>();
        }

        public static ITypeSafeDictionary Create(uint size)
        {
            return new TypeSafeDictionary<T>(size);
        }
    }
}