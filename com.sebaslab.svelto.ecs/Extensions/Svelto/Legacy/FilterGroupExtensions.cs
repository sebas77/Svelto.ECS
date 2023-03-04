#if SVELTO_LEGACY_FILTERS
using Svelto.ECS.Native;

namespace Svelto.ECS
{
    public static class FilterGroupExtensions
    {
        public static bool Add<N>(this LegacyFilterGroup legacyFilter, uint entityID, N mapper) where N : IEGIDMultiMapper
        {
        #if DEBUG && !PROFILE_SVELTO
            if (mapper.Exists(legacyFilter._exclusiveGroupStruct, entityID) == false)
                throw new ECSException(
                    $"trying adding an entity {entityID} to filter {mapper.entityType} - {legacyFilter._ID} with group {legacyFilter._exclusiveGroupStruct}, but entity is not found! ");
        #endif

            return legacyFilter.InternalAdd(entityID, mapper.GetIndex(legacyFilter._exclusiveGroupStruct, entityID));
        }

    }
}
#endif