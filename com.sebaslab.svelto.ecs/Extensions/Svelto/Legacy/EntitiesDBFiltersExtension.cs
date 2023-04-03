#if SVELTO_LEGACY_FILTERS
using Svelto.DataStructures;
using Svelto.ECS.Native;

namespace Svelto.ECS
{
    public static class EntitiesDBFiltersExtension
    {
        public static bool AddEntityToFilter<N>(this EntitiesDB.LegacyFilters legacyFilters, int filtersID, EGID egid, N mapper) where N : IEGIDMultiMapper
        {
            ref var filter =
                ref legacyFilters.CreateOrGetFilterForGroup(filtersID, egid.groupID,  ComponentTypeMap.FetchID(mapper.entityType));

            return filter.Add(egid.entityID, mapper);
        }
    }
}
#endif