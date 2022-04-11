using System.Runtime.CompilerServices;
using Svelto.Common;
using Svelto.DataStructures.Native;
using Svelto.ECS.Native;

namespace Svelto.ECS
{
    public readonly struct EntityFilterCollection
    {
        internal EntityFilterCollection(CombinedFilterID combinedFilterId,
            Allocator allocatorStrategy = Allocator.Persistent)
        {
            _filtersPerGroup =
                SharedSveltoDictionaryNative<ExclusiveGroupStruct, GroupFilters>.Create(allocatorStrategy);

            combinedFilterID = combinedFilterId;
        }

        public CombinedFilterID combinedFilterID { get; }
        
        public EntityFilterIterator GetEnumerator()  => new EntityFilterIterator(this);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Add<T>(EGID egid, NativeEGIDMapper<T> mmap) where T : unmanaged, IEntityComponent
        {
            DBC.ECS.Check.Require(mmap.groupID == egid.groupID, "not compatible NativeEgidMapper used");

            return Add(egid, mmap.GetIndex(egid.entityID));
        }

        public bool Add<T>(EGID egid, NativeEGIDMultiMapper<T> mmap) where T : unmanaged, IEntityComponent
        {
            return Add(egid, mmap.GetIndex(egid));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Add(EGID egid, uint toIndex)
        {
            return GetGroupFilter(egid.groupID).Add(egid.entityID, toIndex);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Add(uint entityID, ExclusiveGroupStruct groupId, uint index)
        {
            Add(new EGID(entityID, groupId), index);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Remove(EGID egid)
        {
            _filtersPerGroup[egid.groupID].Remove(egid.entityID);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Exists(EGID egid)
        {
            return GetGroupFilter(egid.groupID).Exists(egid.entityID);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public GroupFilters GetGroupFilter(ExclusiveGroupStruct group)
        {
            if (_filtersPerGroup.TryGetValue(group, out var groupFilter) == false)
            {
                groupFilter = new GroupFilters(group);
                _filtersPerGroup.Add(group, groupFilter);
            }

            return groupFilter;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Clear()
        {
            var filterSets = _filtersPerGroup.GetValues(out var count);
            for (var i = 0; i < count; i++)
            {
                filterSets[i].Clear();
            }
        }

        internal int groupCount => _filtersPerGroup.count;
        
        public void ComputeFinalCount(out int count)
        {
            count = 0;
            
            for (int i = 0; i < _filtersPerGroup.count; i++)
            {
                count += (int)GetGroup(i).count;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal GroupFilters GetGroup(int indexGroup)
        {
            DBC.ECS.Check.Require(indexGroup < _filtersPerGroup.count);
            return _filtersPerGroup.GetValues(out _)[indexGroup];
        }

        public void Dispose()
        {
            var filterSets = _filtersPerGroup.GetValues(out var count);
            for (var i = 0; i < count; i++)
            {
                filterSets[i].Dispose();
            }

            _filtersPerGroup.Dispose();
        }

        internal readonly SharedSveltoDictionaryNative<ExclusiveGroupStruct, GroupFilters> _filtersPerGroup;

        public struct GroupFilters
        {
            internal GroupFilters(ExclusiveGroupStruct group) : this()
            {
                _entityIDToDenseIndex = new SharedSveltoDictionaryNative<uint, uint>(1);
                _indexToEntityId      = new SharedSveltoDictionaryNative<uint, uint>(1);
                _group                = group;
            }

            public bool Add(uint entityId, uint entityIndex)
            {
                //TODO: when sentinels are finished, we need to add AsWriter here
                if (_entityIDToDenseIndex.TryAdd(entityId, entityIndex, out _))
                {
                    _indexToEntityId[entityIndex] = entityId;
                    return true;
                }

                return false;
            }

            public bool Exists(uint entityId) => _entityIDToDenseIndex.ContainsKey(entityId);

            public void Remove(uint entityId)
            {
                _indexToEntityId.Remove(_entityIDToDenseIndex[entityId]);
                _entityIDToDenseIndex.Remove(entityId);
            }

            public EntityFilterIndices indices
            {
                get
                {
                    var values = _entityIDToDenseIndex.GetValues(out var count);
                    return new EntityFilterIndices(values, count);
                }
            }

            public int count   => _entityIDToDenseIndex.count;
            public bool isValid => _entityIDToDenseIndex.isValid;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal void RemoveWithSwapBack(uint entityId, uint entityIndex, uint lastIndex)
            {
                // Check if the last index is part of the filter as an entity, in that case
                //we need to update the filter
                if (entityIndex != lastIndex && _indexToEntityId.TryGetValue(lastIndex, out var lastEntityID))
                {
                    _entityIDToDenseIndex[lastEntityID] = entityIndex;
                    _indexToEntityId[entityIndex]       = lastEntityID;

                    _indexToEntityId.Remove(lastIndex);
                }
                else
                {
                    // We don't need to check if the entityIndex is a part of the dictionary.
                    // The Remove function will check for us.
                    _indexToEntityId.Remove(entityIndex);
                }

                // We don't need to check if the entityID is part of the dictionary.
                // The Remove function will check for us.
                _entityIDToDenseIndex.Remove(entityId);
            }

            internal void Clear()
            {
                _indexToEntityId.FastClear();
                _entityIDToDenseIndex.FastClear();
            }

            internal void Dispose()
            {
                _entityIDToDenseIndex.Dispose();
                _indexToEntityId.Dispose();
            }

            internal ExclusiveGroupStruct group => _group;

            SharedSveltoDictionaryNative<uint, uint>          _indexToEntityId;
            internal SharedSveltoDictionaryNative<uint, uint> _entityIDToDenseIndex;
            readonly ExclusiveGroupStruct                     _group;
        }
    }
}