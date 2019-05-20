using System.Runtime.CompilerServices;
using Svelto.ECS.Internal;

namespace Svelto.ECS
{
    public struct EGIDMapper<T> where T : struct, IEntityStruct
    {
        internal TypeSafeDictionary<T> map;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref T Entity(uint entityID)
        {
            return ref map.FindElement(entityID);
        }
        
        public bool TryQueryEntity(uint entityID, out T @value)
        {
            if (map.TryFindIndex(entityID, out var index))
            {
                @value = map.GetDirectValue(index);
                return true;
            }

            @value = default;
            return false;
        }
    }
}