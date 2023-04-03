#if SVELTO_LEGACY_FILTERS
using DBC.ECS;
using Svelto.DataStructures;
using Svelto.DataStructures.Native;
using Svelto.ECS.Internal;

namespace Svelto.ECS
{
    /// <summary>
    ///     This feature must be eventually tied to the new ExclusiveGroup that won't allow the use of
    /// custom EntitiesID
    ///     The filters could be updated when entities buffer changes during the submission, while now this process
    ///     is completely manual.
    ///     Making this working properly is not in my priorities right now, as I will need to add the new group type
    ///     AND optimize the submission process to be able to add more overhead
    /// </summary>
    public partial class EntitiesDB
    {
        FasterDictionary<ComponentID, FasterDictionary<ExclusiveGroupStruct, LegacyGroupFilters>> _filters =>
            _enginesRoot._groupFilters;

        public LegacyFilters GetLegacyFilters()
        {
            return new LegacyFilters(_filters);
        }

        public readonly struct LegacyFilters
        {
            public LegacyFilters(
                FasterDictionary<ComponentID, FasterDictionary<ExclusiveGroupStruct, LegacyGroupFilters>>
                    filtersLegacy)
            {
                _filtersLegacy = filtersLegacy;
            }

            public ref LegacyFilterGroup CreateOrGetFilterForGroup<T>(int filterID, ExclusiveGroupStruct groupID)
                where T : struct, _IInternalEntityComponent
            {
                var refWrapper = ComponentTypeID<T>.id;

                return ref CreateOrGetFilterForGroup(filterID, groupID, refWrapper);
            }

            public bool HasFiltersForGroup<T>(ExclusiveGroupStruct groupID) where T : struct, _IInternalEntityComponent
            {
                if (_filtersLegacy.TryGetValue(ComponentTypeID<T>.id, out var fasterDictionary) == false)
                    return false;

                return fasterDictionary.ContainsKey(groupID);
            }

            public bool HasFilterForGroup<T>(int filterID, ExclusiveGroupStruct groupID)
                where T : struct, _IInternalEntityComponent
            {
                if (_filtersLegacy.TryGetValue(ComponentTypeID<T>.id, out var fasterDictionary) == false)
                    return false;

                if (fasterDictionary.TryGetValue(groupID, out var result))
                    return result.HasFilter(filterID);

                return false;
            }

            public ref LegacyGroupFilters CreateOrGetFiltersForGroup<T>(ExclusiveGroupStruct groupID)
                where T : struct, _IInternalEntityComponent
            {
                var fasterDictionary = _filtersLegacy.GetOrAdd(ComponentTypeID<T>.id,
                    () => new FasterDictionary<ExclusiveGroupStruct, LegacyGroupFilters>());

                return ref fasterDictionary.GetOrAdd(groupID,
                    () => new LegacyGroupFilters(new SharedSveltoDictionaryNative<int, LegacyFilterGroup>(0), groupID));
            }

            public ref LegacyGroupFilters GetFiltersForGroup<T>(ExclusiveGroupStruct groupID)
                where T : struct, _IInternalEntityComponent
            {
#if DEBUG && !PROFILE_SVELTO
                if (_filtersLegacy.ContainsKey(ComponentTypeID<T>.id) == false)
                    throw new ECSException($"trying to fetch not existing filters, type {typeof(T)}");
                if (_filtersLegacy[ComponentTypeID<T>.id].ContainsKey(groupID) == false)
                    throw new ECSException(
                        $"trying to fetch not existing filters, type {typeof(T)} group {groupID.ToName()}");
#endif

                return ref _filtersLegacy[ComponentTypeID<T>.id].GetValueByRef(groupID);
            }

            public ref LegacyFilterGroup GetFilterForGroup<T>(int filterId, ExclusiveGroupStruct groupID)
                where T : struct, _IInternalEntityComponent
            {
#if DEBUG && !PROFILE_SVELTO
                if (_filtersLegacy.ContainsKey(ComponentTypeID<T>.id) == false)
                    throw new ECSException($"trying to fetch not existing filters, type {typeof(T)}");
                if (_filtersLegacy[ComponentTypeID<T>.id].ContainsKey(groupID) == false)
                    throw new ECSException(
                        $"trying to fetch not existing filters, type {typeof(T)} group {groupID.ToName()}");
#endif
                return ref _filtersLegacy[ComponentTypeID<T>.id][groupID].GetFilter(filterId);
            }

            public bool TryGetFilterForGroup<T>(int filterId, ExclusiveGroupStruct groupID,
                out LegacyFilterGroup groupLegacyFilter) where T : struct, _IInternalEntityComponent
            {
                groupLegacyFilter = default;

                if (_filtersLegacy.TryGetValue(ComponentTypeID<T>.id, out var fasterDictionary) == false)
                    return false;

                if (fasterDictionary.TryGetValue(groupID, out var groupFilters) == false)
                    return false;

                if (groupFilters.TryGetFilter(filterId, out groupLegacyFilter) == false)
                    return false;

                return true;
            }

            public bool TryGetFiltersForGroup<T>(ExclusiveGroupStruct groupID,
                out LegacyGroupFilters legacyGroupFilters) where T : struct, _IInternalEntityComponent
            {
                legacyGroupFilters = default;

                if (_filtersLegacy.TryGetValue(ComponentTypeID<T>.id, out var fasterDictionary) == false)
                    return false;

                return fasterDictionary.TryGetValue(groupID, out legacyGroupFilters);
            }

            public void ClearFilter<T>(int filterID, ExclusiveGroupStruct exclusiveGroupStruct)
                    where T : struct, _IInternalEntityComponent
            {
                if (_filtersLegacy.TryGetValue(ComponentTypeID<T>.id, out var fasterDictionary))
                {
                    Check.Require(fasterDictionary.ContainsKey(exclusiveGroupStruct),
                        $"trying to clear filter not present in group {exclusiveGroupStruct}");

                    fasterDictionary[exclusiveGroupStruct].ClearFilter(filterID);
                }
            }

            public void ClearFilters<T>(int filterID) where T : struct, _IInternalEntityComponent
            {
                if (_filtersLegacy.TryGetValue(ComponentTypeID<T>.id, out var fasterDictionary))
                    foreach (var filtersPerGroup in fasterDictionary)
                        filtersPerGroup.value.ClearFilter(filterID);
            }

            public void DisposeFilters<T>(ExclusiveGroupStruct exclusiveGroupStruct) where T : struct, _IInternalEntityComponent
            {
                if (_filtersLegacy.TryGetValue(ComponentTypeID<T>.id, out var fasterDictionary))
                {
                    fasterDictionary[exclusiveGroupStruct].DisposeFilters();
                    fasterDictionary.Remove(exclusiveGroupStruct);
                }
            }

            public void DisposeFilters<T>() where T : struct, _IInternalEntityComponent
            {
                if (_filtersLegacy.TryGetValue(ComponentTypeID<T>.id, out var fasterDictionary))
                    foreach (var filtersPerGroup in fasterDictionary)
                        filtersPerGroup.value.DisposeFilters();

                _filtersLegacy.Remove(ComponentTypeID<T>.id);
            }

            public void DisposeFilterForGroup<T>(int resetFilterID, ExclusiveGroupStruct group) where T : struct, _IInternalEntityComponent
            {
                if (_filtersLegacy.TryGetValue(ComponentTypeID<T>.id, out var fasterDictionary))
                    fasterDictionary[@group].DisposeFilter(resetFilterID);
            }

            public bool TryRemoveEntityFromFilter<T>(int filtersID, EGID egid) where T : struct, _IInternalEntityComponent
            {
                if (TryGetFilterForGroup<T>(filtersID, egid.groupID, out var filter))
                    return filter.TryRemove(egid.entityID);

                return false;
            }

            public void RemoveEntityFromFilter<T>(int filtersID, EGID egid) where T : struct, _IInternalEntityComponent
            {
                ref var filter = ref GetFilterForGroup<T>(filtersID, egid.groupID);

                filter.Remove(egid.entityID);
            }

            public bool AddEntityToFilter<N>(int filtersID, EGID egid, N mapper) where N : IEGIDMapper
            {
                ref var filter = ref CreateOrGetFilterForGroup(filtersID, egid.groupID, ComponentTypeMap.FetchID(mapper.entityType));

                return filter.Add(egid.entityID, mapper);
            }

            internal ref LegacyFilterGroup CreateOrGetFilterForGroup(int filterID, ExclusiveGroupStruct groupID,
                ComponentID refWrapper)
            {
                var fasterDictionary = _filtersLegacy.GetOrAdd(refWrapper,
                    () => new FasterDictionary<ExclusiveGroupStruct, LegacyGroupFilters>());

                var filters = fasterDictionary.GetOrAdd(groupID,
                    (ref ExclusiveGroupStruct gid) =>
                        new LegacyGroupFilters(new SharedSveltoDictionaryNative<int, LegacyFilterGroup>(0), gid),
                    ref groupID);

                return ref filters.CreateOrGetFilter(filterID);
            }

            readonly FasterDictionary<ComponentID, FasterDictionary<ExclusiveGroupStruct, LegacyGroupFilters>>
                _filtersLegacy;
        }
    }
}
#endif