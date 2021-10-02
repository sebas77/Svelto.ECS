using Svelto.ECS.Native;

namespace Svelto.ECS
{
    public static class FilterGroupExtensions
    {
        public static bool Add<N>(this FilterGroup filter, uint entityID, N mapper) where N : IEGIDMultiMapper
        {
        #if DEBUG && !PROFILE_SVELTO
            if (mapper.Exists(filter._exclusiveGroupStruct, entityID) == false)
                throw new ECSException(
                    $"trying adding an entity {entityID} to filter {mapper.entityType} - {filter._ID} with group {filter._exclusiveGroupStruct}, but entity is not found! ");
        #endif

            return filter.InternalAdd(entityID, mapper.GetIndex(filter._exclusiveGroupStruct, entityID));
        }

    }
}