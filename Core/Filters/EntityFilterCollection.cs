using System;
using System.Runtime.CompilerServices;
using Svelto.Common;
using Svelto.DataStructures.Native;
using Svelto.ECS.Internal;
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
        public bool Add<T>(EGID egid, NativeEGIDMapper<T> mmap) where T : unmanaged, _IInternalEntityComponent
        {
            DBC.ECS.Check.Require(mmap.groupID == egid.groupID, "not compatible NativeEgidMapper used");

            return Add(egid, mmap.GetIndex(egid.entityID));
        }

        public bool Add<T>(EGID egid, NativeEGIDMultiMapper<T> mmap) where T : unmanaged, _IInternalEntityComponent
        {
            return Add(egid, mmap.GetIndex(egid));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Add(EGID egid, uint indexInComponentArray)
        {
            return GetOrCreateGroupFilter(egid.groupID).Add(egid.entityID, indexInComponentArray);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Add(uint entityID, ExclusiveGroupStruct groupId, uint indexInComponentArray)
        {
            Add(new EGID(entityID, groupId), indexInComponentArray);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Remove(EGID egid)
        {
            _filtersPerGroup[egid.groupID].Remove(egid.entityID);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Remove(uint entityID, ExclusiveGroupStruct groupID)
        {
            _filtersPerGroup[groupID].Remove(entityID);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Exists(EGID egid)
        {
            if (TryGetGroupFilter(egid.groupID, out var groupFilter))
            {
                return groupFilter.Exists(egid.entityID);
            }
            return false;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryGetGroupFilter(ExclusiveGroupStruct group, out GroupFilters groupFilter)
        {
            return _filtersPerGroup.TryGetValue(group, out groupFilter);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public GroupFilters GetOrCreateGroupFilter(ExclusiveGroupStruct group)
        {
            if (_filtersPerGroup.TryGetValue(group, out var groupFilter) == false)
            {
                groupFilter = new GroupFilters(group);
                _filtersPerGroup.Add(group, groupFilter);
            }

            return groupFilter;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public GroupFilters CreateGroupFilter(ExclusiveBuildGroup group)
        {
            if (_filtersPerGroup.TryGetValue(group, out var groupFilter) == false)
            {
                groupFilter = new GroupFilters(group);
                _filtersPerGroup.Add(group, groupFilter);
                
                return groupFilter;
            }

            throw new ECSException("group already linked to filter {group}");
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public GroupFilters GetGroupFilter(ExclusiveGroupStruct group)
        {
            if (_filtersPerGroup.TryGetValue(group, out var groupFilter) == true)
                return groupFilter;

            throw new Exception($"no filter linked to group {group}");
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
                _group                = group;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public bool Add(uint entityId, uint indexInComponentArray)
            {
                //TODO: when sentinels are finished, we need to add AsWriter here
                //cannot write in parallel
                return _entityIDToDenseIndex.TryAdd(entityId, indexInComponentArray, out _);
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public bool Exists(uint entityId) => _entityIDToDenseIndex.ContainsKey(entityId);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Remove(uint entityId)
            {
                _entityIDToDenseIndex.Remove(entityId);
            }

            public EntityFilterIndices indices
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get
                {
                    var values = _entityIDToDenseIndex.GetValues(out var count);
                    return new EntityFilterIndices(values, count);
                }
            }

            public uint this[uint entityId]
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => _entityIDToDenseIndex[entityId];
            }

            public int count   => _entityIDToDenseIndex.count;
            public bool isValid => _entityIDToDenseIndex.isValid;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal void Clear()
            {
                _entityIDToDenseIndex.Clear();
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal void Dispose()
            {
                _entityIDToDenseIndex.Dispose();
            }

            internal ExclusiveGroupStruct group => _group;

            internal SharedSveltoDictionaryNative<uint, uint> _entityIDToDenseIndex;
            readonly ExclusiveGroupStruct                     _group;
        }
    }
}
