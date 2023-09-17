using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Svelto.Common;

namespace Svelto.ECS
{
    public partial class EnginesRoot
    {
        class GenericEntityFactory : IEntityFactory, IEntitySerializationFactory
        {
            public GenericEntityFactory(EnginesRoot weakReference)
            {
                _enginesRoot = new Svelto.DataStructures.WeakReference<EnginesRoot>(weakReference);
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public EntityInitializer BuildEntity<T>
            (uint entityID, ExclusiveBuildGroup groupStructId, IEnumerable<object> implementors = null
           , [CallerMemberName] string caller = null) where T : IEntityDescriptor, new()
            {
                return _enginesRoot.Target.BuildEntity(new EGID(entityID, groupStructId)
                                                     , EntityDescriptorTemplate<T>.realDescriptor.componentsToBuild
                                                     , TypeCache<T>.type, implementors, caller);
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public EntityInitializer BuildEntity<T>
            (EGID egid, IEnumerable<object> implementors = null
           , [CallerMemberName] string caller = null) where T : IEntityDescriptor, new()
            {
                return _enginesRoot.Target.BuildEntity(egid, EntityDescriptorTemplate<T>.realDescriptor.componentsToBuild
                                                     , TypeCache<T>.type, implementors, caller);
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public EntityInitializer BuildEntity<T>
            (EGID egid, T entityDescriptor, IEnumerable<object> implementors
           , [CallerMemberName] string caller = null) where T : IEntityDescriptor
            {
                return _enginesRoot.Target.BuildEntity(egid, entityDescriptor.componentsToBuild, TypeCache<T>.type
                                                     , implementors, caller);
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public EntityInitializer BuildEntity<T>
            (uint entityID, ExclusiveBuildGroup groupStructId, T descriptorEntity, IEnumerable<object> implementors
           , [CallerMemberName] string caller = null) where T : IEntityDescriptor
            {
                return _enginesRoot.Target.BuildEntity(new EGID(entityID, groupStructId)
                                                     , descriptorEntity.componentsToBuild, TypeCache<T>.type
                                                     , implementors, caller);
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public EntityInitializer BuildEntity
            (EGID egid, IComponentBuilder[] componentsToBuild, Type type, IEnumerable<object> implementors = null
              , [CallerMemberName] string caller = null)
            {
                return _enginesRoot.Target.BuildEntity(egid, componentsToBuild, type, implementors, caller);
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void PreallocateEntitySpace<T>(ExclusiveGroupStruct groupStructId, uint numberOfEntities)
                where T : IEntityDescriptor, new()
            {
                _enginesRoot.Target.Preallocate(groupStructId, numberOfEntities
                                              , EntityDescriptorTemplate<T>.realDescriptor.componentsToBuild);
            }

#if UNITY_NATIVE
            public Native.NativeEntityFactory ToNative<T>
                ([CallerMemberName] string caller = null)
                where T : IEntityDescriptor, new()
            {
                return _enginesRoot.Target.ProvideNativeEntityFactoryQueue<T>(caller);
            }
#endif

            //NOTE: enginesRoot is a weakreference ONLY because GenericEntityStreamConsumerFactory can be injected inside
            //engines of other enginesRoot
            readonly Svelto.DataStructures.WeakReference<EnginesRoot> _enginesRoot;
        }
    }
}