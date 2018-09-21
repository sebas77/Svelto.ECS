using System;
using System.Collections.Generic;
using Svelto.DataStructures.Experimental;

namespace Svelto.ECS.Internal
{
    static class EntityFactory
    {
        internal static Dictionary<Type, ITypeSafeDictionary> 
            BuildGroupedEntities(EGID egid,
                 FasterDictionary<int, Dictionary<Type, ITypeSafeDictionary>> groupEntityViewsByType,
                 IEntityBuilder[] entitiesToBuild,
                 object[] implementors)
        {
            var @group = FetchEntityGroup(egid.groupID, groupEntityViewsByType);

            BuildEntitiesAndAddToGroup(egid, group, entitiesToBuild, implementors);

            return group;
        }

        static Dictionary<Type, ITypeSafeDictionary> FetchEntityGroup(int groupID, 
            FasterDictionary<int, Dictionary<Type, ITypeSafeDictionary>> groupEntityViewsByType)
        {
            Dictionary<Type, ITypeSafeDictionary> group;

            if (groupEntityViewsByType.TryGetValue(groupID, out @group) == false)
            {
                @group = new Dictionary<Type, ITypeSafeDictionary>();
                groupEntityViewsByType.Add(groupID, @group);
            }

            return @group;
        }

        static void BuildEntitiesAndAddToGroup(EGID entityID,
            Dictionary<Type, ITypeSafeDictionary> @group,
            IEntityBuilder[] entitiesToBuild,
            object[] implementors)
        {
            var count = entitiesToBuild.Length;
#if DEBUG && !PROFILER
            HashSet<Type> types = new HashSet<Type>();
            
            for (var index = 0; index < count; ++index)
            {
                var entityType = entitiesToBuild[index].GetEntityType();
                if (types.Contains(entityType))
                {
                    throw new ECSException("EntityBuilders must be unique inside an EntityDescriptor");
                }
                
                types.Add(entityType);
            }
#endif            

            for (var index = 0; index < count; ++index)
            {
                var entityViewBuilder = entitiesToBuild[index];
                var entityViewType    = entityViewBuilder.GetEntityType();

                BuildEntity(entityID, @group, entityViewType, entityViewBuilder, implementors);
            }
        }

        static void BuildEntity(EGID  entityID, Dictionary<Type, ITypeSafeDictionary> @group,
                                    Type entityViewType, IEntityBuilder entityBuilder, object[] implementors)
        {
            ITypeSafeDictionary safeDictionary;

            var entityViewsPoolWillBeCreated = @group.TryGetValue(entityViewType, out safeDictionary) == false;

            //passing the undefined entityViewsByType inside the entityViewBuilder will allow
            //it to be created with the correct type and casted back to the undefined list.
            //that's how the list will be eventually of the target type.
            entityBuilder.BuildEntityAndAddToList(ref safeDictionary, entityID, implementors);

            if (entityViewsPoolWillBeCreated)
                @group.Add(entityViewType, safeDictionary);
        }
    }
}