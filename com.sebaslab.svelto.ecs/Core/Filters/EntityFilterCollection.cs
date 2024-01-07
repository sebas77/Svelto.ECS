using System;
using System.Runtime.CompilerServices;
using Svelto.Common;
using Svelto.DataStructures.Native;
using Svelto.ECS.Internal;
using Svelto.ECS.Native;

namespace Svelto.ECS
{
    /// <summary>
    /// note: the whole Svelto ECS framework is based on the assumption that the NB and MB structures are ref
    /// since we are holding them inside jobs, we decided to not mark them as ref but the pointers inside must be constant!
    /// This works because NB and MB data wrapped can be changed only inside a submission which is not jobifiable
    /// however filters can be modified inside jobs which means that data can change asynchronously. For this reason filters should be
    /// always queries inside jobs when these are used.
    /// </summary>
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

        /// <summary>
        /// Add a new entity to this filter
        /// This method assumes that the entity has already been submitted in the database, so it can be found through the EGIDMapper 
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Add<T>(EGID egid, NativeEGIDMapper<T> mmap) where T : unmanaged, _IInternalEntityComponent
        {
            DBC.ECS.Check.Require(mmap.groupID == egid.groupID, "not compatible NativeEgidMapper used");

            Add(egid, mmap.GetIndex(egid.entityID));
        }

        /// <summary>
        /// Add a new entity to this filter
        /// This method assumes that the entity has already been submitted in the database, so it can be found through the EGIDMapper 
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Add<T>(EGID egid, NativeEGIDMultiMapper<T> mmap) where T : unmanaged, _IInternalEntityComponent
        {
            Add(egid, mmap.GetIndex(egid));
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Add<T>(EGID egid, EGIDMapper<T> mmap) where T : unmanaged, _IInternalEntityComponent
        {
            DBC.ECS.Check.Require(mmap.groupID == egid.groupID, "not compatible NativeEgidMapper used");

            Add(egid, mmap.GetIndex(egid.entityID));
        }

        /// <summary>
        /// Add a new entity to this filter
        /// This method assumes that the entity has already been submitted in the database, so it can be found through the EGIDMapper 
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Add<T>(EGID egid, EGIDMultiMapper<T> mmap) where T : unmanaged, _IInternalEntityComponent
        {
            Add(egid, mmap.GetIndex(egid));
        }

        /// <summary>
        /// Add a new entity to this filter
        /// If the user knows at which position the entity is stored in the database, it can be passed directly to the filter
        /// This position can be assumed if the user knows how Svelto stores components internally, but it's only guaranteed when passed back from the
        /// Svelto callbacks, like the IReactOnAddEx.Add callbacks. Exploting IReactOnAddEx is the only reccomended pattern to use together with this
        /// method. If the wrong index is passed, the filter will have undefined behaviour
        /// </summary>
        /// <param name="egid"> the entity EGID </param>
        /// <param name="indexInComponentArray"> the position in the Svelto database array where the component is stored</param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Add(EGID egid, uint indexInComponentArray)
        {
            GetOrCreateGroupFilter(egid.groupID).Add(egid.entityID, indexInComponentArray);
        }

        /// <summary>
        /// Add a new entity to this filter
        /// If the user knows at which position the entity is stored in the database, it can be passed directly to the filter
        /// This position can be assumed if the user knows how Svelto stores components internally, but it's only guaranteed when passed back from the
        /// Svelto callbacks, like the IReactOnAddEx.Add callbacks. Exploting IReactOnAddEx is the only reccomended pattern to use together with this
        /// method. If the wrong index is passed, the filter will have undefined behaviour
        /// </summary>
        /// <param name="egid"> the entity EGID </param>
        /// <param name="indexInComponentArray"> the position in the Svelto database array where the component is stored</param>
        /// <returns></returns>
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
        
        public int ComputeFinalCount()
        {
            int count = 0;
            
            for (int i = 0; i < _filtersPerGroup.count; i++)
            {
                count += (int)GetGroup(i).count;
            }

            return count;
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
                //it's a TryAdd because there is no reason to not replace the index if the entity is already there
                //TODO: when sentinels are finished, we need to add AsWriter here
                //cannot write in parallel
                return _entityIDToDenseIndex.TryAdd(entityId, indexInComponentArray, out _);
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public bool Exists(uint entityId) => _entityIDToDenseIndex.ContainsKey(entityId);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public bool Remove(uint entityId) => _entityIDToDenseIndex.Remove(entityId);

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
