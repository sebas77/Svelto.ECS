using Svelto.DataStructures;
using Svelto.ECS.Native;

namespace Svelto.ECS
{
    public static class EntitiesDBFiltersExtension
    {
        public static bool AddEntityToFilter<N>(this EntitiesDB.Filters filters, int filtersID, EGID egid, N mapper) where N : IEGIDMultiMapper
        {
            ref var filter =
                ref filters.CreateOrGetFilterForGroup(filtersID, egid.groupID, new RefWrapperType(mapper.entityType));

            return filter.Add(egid.entityID, mapper);
        }
    }
}