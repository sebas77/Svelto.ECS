using System;
using System.Runtime.CompilerServices;
using Svelto.ECS.Internal;

namespace Svelto.ECS
{
    public readonly struct EGIDMapper<T> where T : struct, IEntityComponent
    {
        internal readonly ITypeSafeDictionary<T> map;
        public uint Length => map.Count;
        public ExclusiveGroupStruct groupID { get; }

        public EGIDMapper(ExclusiveGroupStruct groupStructId, ITypeSafeDictionary<T> dic):this()
        {
            groupID = groupStructId;
            map = dic;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref T Entity(uint entityID)
        {
#if DEBUG && !PROFILE_SVELTO
                if (map.TryFindIndex(entityID, out var findIndex) == false)
                    throw new Exception("Entity not found in this group ".FastConcat(typeof(T).ToString()));
#else
                map.TryFindIndex(entityID, out var findIndex);
#endif
                return ref map.unsafeValues[(int) findIndex];
        }
        
        public bool TryGetEntity(uint entityID, out T value)
        {
            if (map.TryFindIndex(entityID, out var index))
            {
                value = map.unsafeValues[index];
                return true;
            }

            value = default;
            return false;
        }
        
        public T[] GetArrayAndEntityIndex(uint entityID, out uint index)
        {
            if (map.TryFindIndex(entityID, out index))
            {
                return map.unsafeValues;
            }

            throw new ECSException("Entity not found");
        }
        
        public bool TryGetArrayAndEntityIndex(uint entityID, out uint index, out T[] array)
        {
            if (map.TryFindIndex(entityID, out index))
            {
                array =  map.unsafeValues;
                return true;
            }

            array = default;
            return false;
        }
    }
}

