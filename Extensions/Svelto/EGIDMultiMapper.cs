using System;
using System.Runtime.CompilerServices;
using Svelto.Common;
using Svelto.DataStructures;
using Svelto.ECS.Internal;

namespace Svelto.ECS
{
    /// <summary>
    /// to retrieve an EGIDMultiMapper use entitiesDB.QueryMappedEntities<T>(groups);
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public readonly struct EGIDMultiMapper<T>: IEGIDMultiMapper where T : struct, _IInternalEntityComponent
    {
        internal EGIDMultiMapper(FasterDictionary<ExclusiveGroupStruct, ITypeSafeDictionary<T>> dictionary)
        {
            _dic = dictionary;
        }

        public int count
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _dic.count;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref T Entity(EGID entity)
        {
#if DEBUG && !PROFILE_SVELTO
            if (Exists(entity) == false)
                throw new Exception("EGIDMultiMapper: Entity not found");
#endif
            ref var sveltoDictionary = ref _dic.GetValueByRef(entity.groupID);
            return ref sveltoDictionary.GetValueByRef(entity.entityID);
        }
        
        public EntityCollection<T> Entities(ExclusiveGroupStruct targetEgidGroupID)
        {
            uint count = 0;
            IBuffer<T> buffer;
            IEntityIDs ids = default;

            if (_dic.TryGetValue(targetEgidGroupID, out var typeSafeDictionary) == false)
                buffer = default;
            else
            {
                ITypeSafeDictionary<T> safeDictionary = typeSafeDictionary;
                buffer = safeDictionary.GetValues(out count);
                ids = safeDictionary.entityIDs;
            }

            return new EntityCollection<T>(buffer, ids, count);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Exists(EGID entity)
        {
            return _dic.TryFindIndex(entity.groupID, out var index) && _dic.GetDirectValueByRef(index).ContainsKey(entity.entityID);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryGetEntity(EGID entity, out T component)
        {
            component = default;
            return _dic.TryFindIndex(entity.groupID, out var index)
                 && _dic.GetDirectValueByRef(index).TryGetValue(entity.entityID, out component);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool FindIndex(ExclusiveGroupStruct group, uint entityID, out uint index)
        {
            index = 0;
            return _dic.TryFindIndex(group, out var groupIndex) &&
                    _dic.GetDirectValueByRef(groupIndex).TryFindIndex(entityID, out index);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public uint GetIndex(ExclusiveGroupStruct group, uint entityID)
        {
            uint groupIndex = _dic.GetIndex(group);
            return _dic.GetDirectValueByRef(groupIndex).GetIndex(entityID);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public uint GetIndex(EGID egid)
        {
            return GetIndex(egid.groupID, egid.entityID);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Exists(ExclusiveGroupStruct group, uint entityID)
        {
            return _dic.TryFindIndex(group, out var groupIndex) &&
                    _dic.GetDirectValueByRef(groupIndex).ContainsKey(entityID);
        }

        public Type entityType => TypeCache<T>.type;

        readonly FasterDictionary<ExclusiveGroupStruct, ITypeSafeDictionary<T>> _dic;
    }
}