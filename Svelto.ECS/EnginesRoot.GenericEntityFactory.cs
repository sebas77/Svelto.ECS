using System.Collections.Generic;
using Svelto.DataStructures;
using Svelto.ECS.Internal;

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

            public EntityComponentInitializer BuildEntity<T>
                (uint entityID, ExclusiveGroupStruct groupStructId, IEnumerable<object> implementors = null)
                where T : IEntityDescriptor, new()
            {
                return _enginesRoot.Target.BuildEntity(new EGID(entityID, groupStructId)
                                                     , EntityDescriptorTemplate<T>.descriptor.componentsToBuild
                                                     , implementors);
            }

            public EntityComponentInitializer BuildEntity<T>(EGID egid, IEnumerable<object> implementors = null)
                where T : IEntityDescriptor, new()
            {
                return _enginesRoot.Target.BuildEntity(
                    egid, EntityDescriptorTemplate<T>.descriptor.componentsToBuild
                  , implementors);
            }

            public EntityComponentInitializer BuildEntity<T>
                (EGID egid, T entityDescriptor, IEnumerable<object> implementors) where T : IEntityDescriptor
            {
                return _enginesRoot.Target.BuildEntity(egid, entityDescriptor.componentsToBuild, implementors);
            }
#if UNITY_ECS
            public NativeEntityFactory ToNative<T>(Unity.Collections.Allocator allocator) where T : IEntityDescriptor, new()
            {
                return _enginesRoot.Target.ProvideNativeEntityFactoryQueue<T>();
            }
#endif            
            public EntityComponentInitializer BuildEntity<T>
                (uint entityID, ExclusiveGroupStruct groupStructId, T descriptorEntity, IEnumerable<object> implementors)
                where T : IEntityDescriptor
            {
                return _enginesRoot.Target.BuildEntity(new EGID(entityID, groupStructId)
                                                     , descriptorEntity.componentsToBuild, implementors);
            }

            public void PreallocateEntitySpace<T>(ExclusiveGroupStruct groupStructId, uint size)
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