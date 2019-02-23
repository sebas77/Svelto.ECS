namespace Svelto.ECS
{
    public partial class EnginesRoot
    {
        class GenericEntityFactory : IEntityFactory
        {
            public GenericEntityFactory(DataStructures.WeakReference<EnginesRoot> weakReference)
            {
                _weakEngine = weakReference;
            }

            public EntityStructInitializer BuildEntity<T>(int entityID,  ExclusiveGroup.ExclusiveGroupStruct groupStructId, object[] implementors) where T : IEntityDescriptor, new()
            {
                return _weakEngine.Target.BuildEntity<T>(new EGID(entityID, (int)groupStructId), implementors);
            }

            public EntityStructInitializer BuildEntity<T>(EGID egid, object[] implementors) where T : IEntityDescriptor, new()
            {
                return _weakEngine.Target.BuildEntity<T>(egid, implementors);
            }

            public EntityStructInitializer BuildEntity<T>(EGID egid, T entityDescriptor, object[] implementors)  where T:IEntityDescriptor
            {
                return _weakEngine.Target.BuildEntity(egid, entityDescriptor, implementors);
            }

            public EntityStructInitializer BuildEntity<T>(int entityID,  ExclusiveGroup.ExclusiveGroupStruct groupStructId, T descriptorEntity, object[] implementors)  where T:IEntityDescriptor
            {
                return _weakEngine.Target.BuildEntity(new EGID(entityID, (int)groupStructId), descriptorEntity, implementors);
            }
            
            public void PreallocateEntitySpace<T>(ExclusiveGroup.ExclusiveGroupStruct groupStructId, int size) where T : IEntityDescriptor, new()
            {
                _weakEngine.Target.Preallocate<T>(groupStructId, size);
            }
            
            readonly DataStructures.WeakReference<EnginesRoot> _weakEngine;
        }
    }
}