using Svelto.ECS.Internal;

namespace Svelto.ECS
{
    public struct EGIDMapper<T> where T : IEntityStruct
    {
        internal TypeSafeDictionary<T> map;

        public uint this[EGID index]
        {
            get { return map.FindElementIndex(index.entityID); }
        }
    }
}