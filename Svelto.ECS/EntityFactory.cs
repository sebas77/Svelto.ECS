using System;
using System.Collections.Generic;

namespace Svelto.ECS.Internal
{
    static class EntityFactory
    {
        internal static Dictionary<Type, ITypeSafeDictionary> BuildGroupedEntities(EGID egid,
            EnginesRoot.DoubleBufferedEntitiesToAdd groupEntitiesToAdd, IEntityBuilder[] entitiesToBuild,
            object[] implementors)
        {
            var @group = FetchEntityGroup(egid.groupID, groupEntitiesToAdd);

            BuildEntitiesAndAddToGroup(egid, group, entitiesToBuild, implementors);

            return group;
        }

        static Dictionary<Type, ITypeSafeDictionary> FetchEntityGroup(uint groupID,
            EnginesRoot.DoubleBufferedEntitiesToAdd groupEntityViewsByType)
        {
            if (groupEntityViewsByType.current.TryGetValue(groupID, out Dictionary<Type, ITypeSafeDictionary> @group) ==
                false)
            {
                @group = new Dictionary<Type, ITypeSafeDictionary>();
                
                groupEntityViewsByType.current.Add(groupID, @group);
            }

            groupEntityViewsByType.currentEntitiesCreatedPerGroup.TryGetValue(groupID, out var value);
            groupEntityViewsByType.currentEntitiesCreatedPerGroup[groupID] = value+1;
            
            return @group;
        }

        static void BuildEntitiesAndAddToGroup(EGID                                    entityID,
                                               Dictionary<Type, ITypeSafeDictionary>   @group,
                                               IEntityBuilder[]                        entitiesToBuild,
                                               object[]                                implementors)
        {
            var count = entitiesToBuild.Length;
#if DEBUG && !PROFILER
            HashSet<Type> types = new HashSet<Type>();
            
            for (var index = 0; index < count; ++index)
            {
                var entityViewType = entitiesToBuild[index].GetEntityType();
                if (types.Contains(entityViewType))
                {
                    throw new ECSException("EntityBuilders must be unique inside an EntityDescriptor");
                }
                
                types.Add(entityViewType);
            }
#endif
            for (var index = 0; index < count; ++index)
            {
                var entityViewBuilder = entitiesToBuild[index];
                var entityViewType = entityViewBuilder.GetEntityType();

                BuildEntity(entityID, @group, entityViewType, entityViewBuilder, implementors);
            }
        }

        static void BuildEntity(EGID entityID, Dictionary<Type, ITypeSafeDictionary> @group, Type entityViewType,
            IEntityBuilder entityBuilder, object[] implementors)
        {
            var entityViewsPoolWillBeCreated = @group.TryGetValue(entityViewType, out var safeDictionary) == false;

            //passing the undefined entityViewsByType inside the entityViewBuilder will allow it to be created with the
            //correct type and casted back to the undefined list. that's how the list will be eventually of the target
            //type.
            entityBuilder.BuildEntityAndAddToList(ref safeDictionary, entityID, implementors);

            if (entityViewsPoolWillBeCreated)
                @group.Add(entityViewType, safeDictionary);
        }
    }
}