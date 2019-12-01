using System;
using System.Collections.Generic;
using Svelto.DataStructures;

namespace Svelto.ECS.Internal
{
    static class EntityFactory
    {
        public static FasterDictionary<RefWrapper<Type>, ITypeSafeDictionary> BuildGroupedEntities(EGID egid,
            EnginesRoot.DoubleBufferedEntitiesToAdd groupEntitiesToAdd,
            IEntityBuilder[] entitiesToBuild,
            IEnumerable<object> implementors)
        {
            var group = FetchEntityGroup(egid.groupID, groupEntitiesToAdd);

            BuildEntitiesAndAddToGroup(egid, group, entitiesToBuild, implementors);

            return group;
        }

        static FasterDictionary<RefWrapper<Type>, ITypeSafeDictionary> FetchEntityGroup(uint groupID,
            EnginesRoot.DoubleBufferedEntitiesToAdd groupEntityViewsByType)
        {
            if (groupEntityViewsByType.current.TryGetValue(groupID, out var group) == false)
            {
                group = new FasterDictionary<RefWrapper<Type>, ITypeSafeDictionary>();
                
                groupEntityViewsByType.current.Add(groupID, group);
            }

            if (groupEntityViewsByType.currentEntitiesCreatedPerGroup.TryGetValue(groupID, out var value) == false)
                groupEntityViewsByType.currentEntitiesCreatedPerGroup[groupID] = 0;
            else
                groupEntityViewsByType.currentEntitiesCreatedPerGroup[groupID] = value+1;
            
            return group;
        }

        static void BuildEntitiesAndAddToGroup(EGID entityID,
            FasterDictionary<RefWrapper<Type>, ITypeSafeDictionary> group,
            IEntityBuilder[] entityBuilders, IEnumerable<object> implementors)
        {
#if DEBUG && !PROFILER
            HashSet<Type> types = new HashSet<Type>();
#endif
            InternalBuild(entityID, group, entityBuilders, implementors
#if DEBUG && !PROFILER
                , types
#endif
            );
        }

        static void InternalBuild(EGID entityID, FasterDictionary<RefWrapper<Type>, ITypeSafeDictionary> group,
            IEntityBuilder[] entityBuilders, IEnumerable<object> implementors
#if DEBUG && !PROFILER
            , HashSet<Type> types
#endif
        )
        {
            var count = entityBuilders.Length;
#if DEBUG && !PROFILER
            for (var index = 0; index < count; ++index)
            {
                var entityViewType = entityBuilders[index].GetEntityType();
                if (types.Contains(entityViewType))
                {
                    throw new ECSException("EntityBuilders must be unique inside an EntityDescriptor");
                }

                types.Add(entityViewType);
            }
#endif
            for (var index = 0; index < count; ++index)
            {
                var entityStructBuilder = entityBuilders[index];
                var entityViewType = entityStructBuilder.GetEntityType();

                BuildEntity(entityID, group, entityViewType, entityStructBuilder, implementors);
            }
        }

        static void BuildEntity(EGID entityID, FasterDictionary<RefWrapper<Type>, ITypeSafeDictionary> group,
            Type entityViewType, IEntityBuilder entityBuilder, IEnumerable<object> implementors)
        {
            var entityViewsPoolWillBeCreated =
                group.TryGetValue(new RefWrapper<Type>(entityViewType), out var safeDictionary) == false;

            //passing the undefined entityViewsByType inside the entityViewBuilder will allow it to be created with the
            //correct type and casted back to the undefined list. that's how the list will be eventually of the target
            //type.
            entityBuilder.BuildEntityAndAddToList(ref safeDictionary, entityID, implementors);

            if (entityViewsPoolWillBeCreated)
                group.Add(new RefWrapper<Type>(entityViewType), safeDictionary);
        }
    }
}