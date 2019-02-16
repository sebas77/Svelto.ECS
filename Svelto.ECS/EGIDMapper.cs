using Svelto.ECS.Internal;

namespace Svelto.ECS
{
    public struct EGIDMapper<T> where T : IEntityStruct
    {
        internal TypeSafeDictionary<T> map;

        public ref T entity(EGID id)
        {
                int count;
                var index = map.FindElementIndex(id.entityID); 
                return ref map.GetValuesArray(out count)[index];
        }
    }
}