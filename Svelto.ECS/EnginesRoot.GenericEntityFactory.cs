using Svelto.ECS.Internal;

#if ENGINE_PROFILER_ENABLED && UNITY_EDITOR
using Svelto.ECS.Profiler;
#endif

namespace Svelto.ECS
{
    public partial class EnginesRoot
    {
        class GenericEntityFactory : IEntityFactory
        {
            readonly DataStructures.WeakReference<EnginesRoot> _weakEngine;

            public GenericEntityFactory(DataStructures.WeakReference<EnginesRoot> weakReference)
            {
                _weakEngine = weakReference;
            }

            public EntityStructInitializer BuildEntity<T>(int entityID, object[] implementors) where T : IEntityDescriptor, new()
            {
                return _weakEngine.Target.BuildEntity<T>(new EGID(entityID), implementors);
            }

            public EntityStructInitializer BuildEntity<T>(int entityID, int groupID, object[] implementors) where T : IEntityDescriptor, new()
            {
                return _weakEngine.Target.BuildEntity<T>(new EGID(entityID, groupID), implementors);
            }

            public EntityStructInitializer BuildEntity<T>(EGID egid, object[] implementors) where T : IEntityDescriptor, new()
            {
                return _weakEngine.Target.BuildEntity<T>(egid, implementors);
            }

            public EntityStructInitializer BuildEntity(int entityID, IEntityViewBuilder[] entityViewsToBuild, object[] implementors)
            {
                return _weakEngine.Target.BuildEntity(new EGID(entityID), entityViewsToBuild, implementors);
            }

            public EntityStructInitializer BuildEntity(EGID egid, IEntityViewBuilder[] entityViewsToBuild, object[] implementors)
            {
                return _weakEngine.Target.BuildEntity(egid, entityViewsToBuild, implementors);
            }

            public EntityStructInitializer BuildEntity(int entityID, int groupID, IEntityViewBuilder[] entityViewsToBuild, object[] implementors)
            {
                return _weakEngine.Target.BuildEntity(new EGID(entityID, groupID), entityViewsToBuild, implementors);
            }
            
            public void PreallocateEntitySpace<T>(int size) where T : IEntityDescriptor, new()
            {
                _weakEngine.Target.Preallocate<T>(ExclusiveGroups.StandardEntity, size);
            }
            
            public void PreallocateEntitySpace<T>(int groupID, int size) where T : IEntityDescriptor, new()
            {
                _weakEngine.Target.Preallocate<T>(groupID, size);
            }
        }
    }
}