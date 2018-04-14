using System;
using System.Collections.Generic;
using Svelto.DataStructures;
using Svelto.Utilities;
using Console = Utility.Console;

namespace Svelto.ECS.Internal
{
    static class EntityFactory
    {
        internal static void BuildGroupedEntityViews(int entityID, int groupID,
                                                     Dictionary<int, Dictionary<Type, ITypeSafeList>> groupEntityViewsByType,
                                                     EntityDescriptorInfo entityViewsToBuildDescriptor,
                                                     Dictionary<long, IEntityViewBuilder[]> entityInfos,
                                                     object[] implementors)
        {
            var @group = FetchGroup(groupID, groupEntityViewsByType);

            BuildEntityViewsAndAddToGroup(new EGID(entityID, groupID), group, entityViewsToBuildDescriptor, implementors);
            
            entityInfos.Add(new EGID(entityID, groupID).GID, entityViewsToBuildDescriptor.entityViewsToBuild);
        }

        static Dictionary<Type, ITypeSafeList> FetchGroup(int groupID, Dictionary<int, Dictionary<Type, ITypeSafeList>> groupEntityViewsByType)
        {
            Dictionary<Type, ITypeSafeList> group;

            if (groupEntityViewsByType.TryGetValue(groupID, out @group) == false)
            {
                @group = new Dictionary<Type, ITypeSafeList>();
                groupEntityViewsByType.Add(groupID, @group);
            }

            return @group;
        }

        static void BuildEntityViewsAndAddToGroup(EGID entityID,
            Dictionary<Type, ITypeSafeList> entityViewsByType,
            EntityDescriptorInfo entityViewsToBuildDescriptor,
            object[] implementors)
        {
            var entityViewsToBuild           = entityViewsToBuildDescriptor.entityViewsToBuild;
            var count                        = entityViewsToBuild.Length;

            for (var index = 0; index < count; index++)
            {
                var entityViewBuilder = entityViewsToBuild[index];
                var entityViewType    = entityViewBuilder.GetEntityViewType();

                BuildEntityView(entityID, entityViewsByType, entityViewType, entityViewBuilder, implementors);
            }
        }

        static void BuildEntityView(EGID  entityID,       Dictionary<Type, ITypeSafeList> entityViewsByType,
                                           Type entityViewType, IEntityViewBuilder entityViewBuilder, object[] implementors)
        {
            ITypeSafeList entityViewsList;

            var entityViewsPoolWillBeCreated =
                entityViewsByType.TryGetValue(entityViewType, out entityViewsList) == false;

            IEntityData entityViewObjectToFill;

            //passing the undefined entityViewsByType inside the entityViewBuilder will allow
            //it to be created with the correct type and casted back to the undefined list.
            //that's how the list will be eventually of the target type.
            entityViewBuilder.BuildEntityViewAndAddToList(ref entityViewsList, entityID, implementors);

            if (entityViewsPoolWillBeCreated)
                entityViewsByType.Add(entityViewType, entityViewsList);
        }
    }
}