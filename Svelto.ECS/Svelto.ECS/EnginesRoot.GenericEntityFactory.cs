using System.Collections.Generic;
using Svelto.DataStructures;

namespace Svelto.ECS
{
    public partial class EnginesRoot
    {
        class GenericEntityFactory : IEntityFactory
        {
            public GenericEntityFactory(EnginesRoot weakReference)
            {
                _enginesRoot = new WeakReference<EnginesRoot>(weakReference);
            }

            public EntityStructInitializer BuildEntity<T>(uint entityID,
                ExclusiveGroup.ExclusiveGroupStruct groupStructId, IEnumerable<object> implementors = null)
                where T : IEntityDescriptor, new()
            {
                return _enginesRoot.Target.BuildEntity(new EGID(entityID, groupStructId),
                    EntityDescriptorTemplate<T>.descriptor.entitiesToBuild, implementors);
            }

            public EntityStructInitializer BuildEntity<T>(EGID egid, IEnumerable<object> implementors = null)
                where T : IEntityDescriptor, new()
            {
                return _enginesRoot.Target.BuildEntity(egid,
                    EntityDescriptorTemplate<T>.descriptor.entitiesToBuild, implementors);
            }

            public EntityStructInitializer BuildEntity<T>(EGID egid, T entityDescriptor,
                IEnumerable<object> implementors)
                where T : IEntityDescriptor
            {
                return _enginesRoot.Target.BuildEntity(egid, entityDescriptor.entitiesToBuild, implementors);
            }

            public EntityStructInitializer BuildEntity<T>(uint entityID,
                ExclusiveGroup.ExclusiveGroupStruct groupStructId, T descriptorEntity, IEnumerable<object> implementors)
                where T : IEntityDescriptor
            {
                return _enginesRoot.Target.BuildEntity(new EGID(entityID, groupStructId),
                    descriptorEntity.entitiesToBuild,
                    implementors);
            }

            public void PreallocateEntitySpace<T>(ExclusiveGroup.ExclusiveGroupStruct groupStructId, uint size)
                where T : IEntityDescriptor, new()
            {
                _enginesRoot.Target.Preallocate<T>(groupStructId, size);
            }

            //enginesRoot is a weakreference because GenericEntityStreamConsumerFactory can be injected inside
//engines of other enginesRoot
            readonly WeakReference<EnginesRoot> _enginesRoot;
        }
    }
}