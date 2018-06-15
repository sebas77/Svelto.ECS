using System;
using System.Collections.Generic;

namespace Svelto.ECS.Internal
{
    static class EntityFactory
    {
        internal static Dictionary<Type, ITypeSafeDictionary> BuildGroupedEntityViews(EGID egid,
                                                     Dictionary<int, Dictionary<Type, ITypeSafeDictionary>> groupEntityViewsByType,
                                                     IEntityBuilder[] entityToBuild,
                                                     object[] implementors)
        {
            var @group = FetchEntityViewGroup(egid.groupID, groupEntityViewsByType);

            BuildEntityViewsAndAddToGroup(egid, group, entityToBuild, implementors);

            return group;
        }

        static Dictionary<Type, ITypeSafeDictionary> FetchEntityViewGroup(int groupID, Dictionary<int, Dictionary<Type, ITypeSafeDictionary>> groupEntityViewsByType)
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

            Builder._initializer = new EntityInfoView {entityToBuild = entityToBuild};
            BuildEntityView(entityID, @group, _viewType, Builder, null);
        }

        static void BuildEntityView(EGID  entityID, Dictionary<Type, ITypeSafeDictionary> @group,
                                    Type entityViewType, IEntityBuilder entityBuilder, object[] implementors)
        {
            ITypeSafeDictionary safeDictionary;

            var entityViewsPoolWillBeCreated =
                @group.TryGetValue(entityViewType, out safeDictionary) == false;

            //passing the undefined entityViewsByType inside the entityViewBuilder will allow
            //it to be created with the correct type and casted back to the undefined list.
            //that's how the list will be eventually of the target type.
            entityBuilder.BuildEntityViewAndAddToList(ref safeDictionary, entityID, implementors);

            if (entityViewsPoolWillBeCreated)
                @group.Add(entityViewType, safeDictionary);
        }
        
        static readonly EntityBuilder<EntityInfoView> Builder = new EntityBuilder<EntityInfoView>();
        static readonly Type                              _viewType = typeof(EntityInfoView);
    }
}