using Svelto.ECS.Internal;

namespace Svelto.ECS
{
    public struct EGIDMapper<T> where T : IEntityStruct
    {
        internal TypeSafeDictionary<T> map;

        public T[] entities(EGID id, out uint index)
        {
                int count;
                index = map.FindElementIndex(id.entityID); 
                return map.GetValuesArray(out count);
        }
    }
}