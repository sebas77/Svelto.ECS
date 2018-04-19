using System;
using System.Collections.Generic;

namespace Svelto.ECS.Internal
{
    static class EntityFactory
    {
        internal static void BuildGroupedEntityViews(EGID egid,
                                                     Dictionary<int, Dictionary<Type, ITypeSafeDictionary>> groupEntityViewsByType,
                                                     EntityDescriptorInfo entityViewsToBuildDescriptor,
                                                     object[] implementors)
        {
            var @group = FetchGroup(egid.groupID, groupEntityViewsByType);

            BuildEntityViewsAndAddToGroup(egid, group, entityViewsToBuildDescriptor, implementors);
        }

        static Dictionary<Type, ITypeSafeDictionary> FetchGroup(int groupID, Dictionary<int, Dictionary<Type, ITypeSafeDictionary>> groupEntityViewsByType)
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
            Dictionary<Type, ITypeSafeDictionary> entityViewsByType,
            EntityDescriptorInfo entityViewsToBuildDescriptor,
            object[] implementors)
        {
            var entityViewsToBuild           = entityViewsToBuildDescriptor.entityViewsToBuild;
            var count                        = entityViewsToBuild.Length;

            for (var index = 0; index < count; ++index)
            {
                var entityViewBuilder = entityViewsToBuild[index];
                var entityViewType    = entityViewBuilder.GetEntityViewType();

                BuildEntityView(entityID, entityViewsByType, entityViewType, entityViewBuilder, implementors);
            }

            _viewBuilder._initializer = new EntityInfoView() {entityViewsToBuild = entityViewsToBuild};
            BuildEntityView(entityID, entityViewsByType, _viewType, _viewBuilder, null);
        }

        static void BuildEntityView(EGID  entityID, Dictionary<Type, ITypeSafeDictionary> entityViewsByType,
                                    Type entityViewType, IEntityViewBuilder entityViewBuilder, object[] implementors)
        {
            ITypeSafeDictionary safeDictionary;

            var entityViewsPoolWillBeCreated =
                entityViewsByType.TryGetValue(entityViewType, out safeDictionary) == false;

            //passing the undefined entityViewsByType inside the entityViewBuilder will allow
            //it to be created with the correct type and casted back to the undefined list.
            //that's how the list will be eventually of the target type.
            entityViewBuilder.BuildEntityViewAndAddToList(ref safeDictionary, entityID, implementors);

            if (entityViewsPoolWillBeCreated)
                entityViewsByType.Add(entityViewType, safeDictionary);
        }
        
        static readonly EntityViewBuilder<EntityInfoView> _viewBuilder = new EntityViewBuilder<EntityInfoView>();
        static readonly Type                              _viewType = typeof(EntityInfoView);
    }
}