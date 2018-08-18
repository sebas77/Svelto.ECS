using System;
using System.Collections.Generic;
using Svelto.DataStructures.Experimental;

namespace Svelto.ECS.Internal
{
    static class EntityFactory
    {
        internal static Dictionary<Type, ITypeSafeDictionary> 
            BuildGroupedEntityViews(EGID egid,
                 FasterDictionary<int, Dictionary<Type, ITypeSafeDictionary>> groupEntityViewsByType,
                 IEntityBuilder[] entityToBuild,
                 object[] implementors)
        {
            var @group = FetchEntityViewGroup(egid.groupID, groupEntityViewsByType);

            BuildEntityViewsAndAddToGroup(egid, group, entityToBuild, implementors);

            return group;
        }

        static Dictionary<Type, ITypeSafeDictionary> FetchEntityViewGroup(int groupID, 
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

        static void BuildEntityViewsAndAddToGroup(EGID entityID,
            Dictionary<Type, ITypeSafeDictionary> @group,
            IEntityBuilder[] entityToBuild,
            object[] implementors)
        {
            var count = entityToBuild.Length;

            for (var index = 0; index < count; ++index)
            {
                var entityViewBuilder = entityToBuild[index];
                var entityViewType    = entityViewBuilder.GetEntityType();

                BuildEntityView(entityID, @group, entityViewType, entityViewBuilder, implementors);
            }
        }

        static void BuildEntityView(EGID  entityID, Dictionary<Type, ITypeSafeDictionary> @group,
                                    Type entityViewType, IEntityBuilder entityBuilder, object[] implementors)
        {
            ITypeSafeDictionary safeDictionary;

            var entityViewsPoolWillBeCreated = @group.TryGetValue(entityViewType, out safeDictionary) == false;

            //passing the undefined entityViewsByType inside the entityViewBuilder will allow
            //it to be created with the correct type and casted back to the undefined list.
            //that's how the list will be eventually of the target type.
            entityBuilder.BuildEntityViewAndAddToList(ref safeDictionary, entityID, implementors);

            if (entityViewsPoolWillBeCreated)
                @group.Add(entityViewType, safeDictionary);
        }
    }
}