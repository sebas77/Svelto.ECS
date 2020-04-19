using System;
using System.Collections.Generic;
using Svelto.DataStructures;

namespace Svelto.ECS.Internal
{
    static class EntityFactory
    {
        public static FasterDictionary<RefWrapper<Type>, ITypeSafeDictionary> BuildGroupedEntities(EGID egid,
            EnginesRoot.DoubleBufferedEntitiesToAdd groupEntitiesToAdd, IComponentBuilder[] componentsToBuild,
            IEnumerable<object> implementors)
        {
            var group = FetchEntityGroup(egid.groupID, groupEntitiesToAdd);

            BuildEntitiesAndAddToGroup(egid, group, componentsToBuild, implementors);

            return group;
        }

        static FasterDictionary<RefWrapper<Type>, ITypeSafeDictionary> FetchEntityGroup(uint groupID,
            EnginesRoot.DoubleBufferedEntitiesToAdd groupEntityComponentsByType)
        {
            if (groupEntityComponentsByType.current.TryGetValue(groupID, out var group) == false)
            {
                group = new FasterDictionary<RefWrapper<Type>, ITypeSafeDictionary>();
                
                groupEntityComponentsByType.current.Add(groupID, group);
            }

            if (groupEntityComponentsByType.currentEntitiesCreatedPerGroup.TryGetValue(groupID, out var value) == false)
                groupEntityComponentsByType.currentEntitiesCreatedPerGroup[groupID] = 0;
            else
                groupEntityComponentsByType.currentEntitiesCreatedPerGroup[groupID] = value+1;
            
            return group;
        }

        static void BuildEntitiesAndAddToGroup(EGID entityID,
            FasterDictionary<RefWrapper<Type>, ITypeSafeDictionary> group,
            IComponentBuilder[] entityBuilders, IEnumerable<object> implementors)
        {
#if DEBUG && !PROFILE_SVELTO
            HashSet<Type> types = new HashSet<Type>();
#endif
            var count = entityBuilders.Length;
#if DEBUG && !PROFILE_SVELTO
            for (var index = 0; index < count; ++index)
            {
                var entityComponentType = entityBuilders[index].GetEntityComponentType();
                if (types.Contains(entityComponentType))
                {
                    throw new ECSException("EntityBuilders must be unique inside an EntityDescriptor");
                }

                types.Add(entityComponentType);
            }
#endif
            for (var index = 0; index < count; ++index)
            {
                var entityComponentBuilder = entityBuilders[index];
                var entityComponentType      = entityComponentBuilder.GetEntityComponentType();

                BuildEntity(entityID, @group, entityComponentType, entityComponentBuilder, implementors);
            }
        }

        static void BuildEntity(EGID entityID, FasterDictionary<RefWrapper<Type>, ITypeSafeDictionary> group,
                                Type entityComponentType, IComponentBuilder componentBuilder, IEnumerable<object> implementors)
        {
            var entityComponentsPoolWillBeCreated =
                group.TryGetValue(new RefWrapper<Type>(entityComponentType), out var safeDictionary) == false;

            //passing the undefined entityComponentsByType inside the entityComponentBuilder will allow it to be created with the
            //correct type and casted back to the undefined list. that's how the list will be eventually of the target
            //type.
            componentBuilder.BuildEntityAndAddToList(ref safeDictionary, entityID, implementors);

            if (entityComponentsPoolWillBeCreated)
                group.Add(new RefWrapper<Type>(entityComponentType), safeDictionary);
        }
    }
}