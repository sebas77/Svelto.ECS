using System.Runtime.CompilerServices;
using Svelto.DataStructures;
using Svelto.ECS.Internal;

namespace Svelto.ECS
{
    public readonly struct EGIDMapper<T> where T : struct, IEntityComponent
    {
        public   uint                   length  => _map.count;
        public   ExclusiveGroupStruct   groupID { get; }

        public EGIDMapper(ExclusiveGroupStruct groupStructId, ITypeSafeDictionary<T> dic) : this()
        {
            groupID = groupStructId;
            _map     = dic;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref T Entity(uint entityID)
        {
#if DEBUG && !PROFILE_SVELTO
                if (_map.TryFindIndex(entityID, out var findIndex) == false)
                    throw new System.Exception("Entity not found in this group ".FastConcat(typeof(T).ToString()));
#else
            _map.TryFindIndex(entityID, out var findIndex);
#endif
            return ref _map.GetDirectValueByRef(findIndex);
        }

        public bool TryGetEntity(uint entityID, out T value)
        {
            if (_map != null && _map.TryFindIndex(entityID, out var index))
            {
                value = _map.GetDirectValueByRef(index);
                return true;
            }

            value = default;
            return false;
        }

        public IBuffer<T> GetArrayAndEntityIndex(uint entityID, out uint index)
        {
            if (_map.TryFindIndex(entityID, out index))
            {
                return _map.GetValues(out _);
            }

            throw new ECSException("Entity not found");
        }

        public bool TryGetArrayAndEntityIndex(uint entityID, out uint index, out IBuffer<T> array)
        {
            index = default;
            if (_map != null && _map.TryFindIndex(entityID, out index))
            {
                array = _map.GetValues(out _);
                return true;
            }

            array = default;
            return false;
        }
        
        readonly ITypeSafeDictionary<T> _map;
    }
}