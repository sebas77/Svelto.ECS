#if UNITY_NATIVE
using System;
using DBC.ECS;
using Svelto.Common;
using Svelto.DataStructures;
using Svelto.ECS.Internal;
using Svelto.ECS.Native;

namespace Svelto.ECS
{
    public partial class EnginesRoot
    {
        NativeEntityRemove ProvideNativeEntityRemoveQueue<T>(string memberName) where T : IEntityDescriptor, new()
        {
            //DBC.ECS.Check.Require(EntityDescriptorTemplate<T>.descriptor.isUnmanaged(), "can't remove entities with not native types");
            //todo: remove operation array and store entity descriptor hash in the return value
            //todo I maybe able to provide a  _nativeSwap.SwapEntity<entityDescriptor>
            //todo make this work with base descriptors too
            var descriptorComponentsToRemove = EntityDescriptorTemplate<T>.descriptor.componentsToBuild;
            
            _nativeRemoveOperations.Add(new NativeOperationRemove(
                                            descriptorComponentsToRemove, TypeCache<T>.type
                                          , memberName));

            return new NativeEntityRemove(_nativeRemoveOperationQueue, _nativeRemoveOperations.count - 1);
        }

        NativeEntitySwap ProvideNativeEntitySwapQueue<T>(string memberName) where T : IEntityDescriptor, new()
        {
            // DBC.ECS.Check.Require(EntityDescriptorTemplate<T>.descriptor.isUnmanaged(), "can't swap entities with not native types");
            //todo: remove operation array and store entity descriptor hash in the return value
            _nativeSwapOperations.Add(new NativeOperationSwap(EntityDescriptorTemplate<T>.descriptor.componentsToBuild
                                                            , TypeCache<T>.type, memberName));

            return new NativeEntitySwap(_nativeSwapOperationQueue, _nativeSwapOperations.count - 1);
        }

        NativeEntityFactory ProvideNativeEntityFactoryQueue<T>(string caller) where T : IEntityDescriptor, new()
        {
            DBC.ECS.Check.Require(EntityDescriptorTemplate<T>.descriptor.IsUnmanaged()
                                , "can't build entities with not native types");
            DBC.ECS.Check.Require(string.IsNullOrEmpty(caller) == false, "an invalid caller has been provided");
            //todo: remove operation array and store entity descriptor hash in the return value
            _nativeAddOperations.Add(new NativeOperationBuild(EntityDescriptorTemplate<T>.descriptor.componentsToBuild
                                                            , TypeCache<T>.type, caller));

            return new NativeEntityFactory(_nativeAddOperationQueue, _nativeAddOperations.count - 1, _entityLocator);
        }

        void FlushNativeOperations(in PlatformProfiler profiler)
        {
            using (profiler.Sample("Native Remove Operations"))
            {
                var removeBuffersCount = _nativeRemoveOperationQueue.count;
                //todo, I don't like that this scans all the queues even if they are empty
                for (int i = 0; i < removeBuffersCount; i++)
                {
                    ref var buffer = ref _nativeRemoveOperationQueue.GetBag(i);

                    while (buffer.IsEmpty() == false)
                    {
                        var componentsIndex = buffer.Dequeue<uint>();
                        var entityEGID      = buffer.Dequeue<EGID>();

                        ref NativeOperationRemove nativeRemoveOperation = ref _nativeRemoveOperations[componentsIndex];

                        CheckRemoveEntityID(entityEGID, nativeRemoveOperation.entityDescriptorType
                                          , nativeRemoveOperation.caller);

                        QueueRemoveEntityOperation(
                            entityEGID, FindRealComponents(entityEGID, nativeRemoveOperation.components)
                          , nativeRemoveOperation.caller);
                    }
                }
            }

            using (profiler.Sample("Native Swap Operations"))
            {
                var swapBuffersCount = _nativeSwapOperationQueue.count;
                for (int i = 0; i < swapBuffersCount; i++)
                {
                    ref var buffer = ref _nativeSwapOperationQueue.GetBag(i);

                    while (buffer.IsEmpty() == false)
                    {
                        var componentsIndex = buffer.Dequeue<uint>();
                        var entityEGID      = buffer.Dequeue<DoubleEGID>();

                        ref var nativeSwapOperation = ref _nativeSwapOperations[componentsIndex];

                        CheckSwapEntityID(entityEGID.@from, entityEGID.to, nativeSwapOperation.entityDescriptorType
                                          , nativeSwapOperation.caller);

                        QueueSwapEntityOperation(entityEGID.@from, entityEGID.to
                                               , FindRealComponents(entityEGID.@from, nativeSwapOperation.components)
                                               , nativeSwapOperation.caller);
                    }
                }
            }

            //todo: it feels weird that these builds in the transient entities database while it could build directly to the final one 
            using (profiler.Sample("Native Add Operations"))
            {
                var addBuffersCount = _nativeAddOperationQueue.count;
                for (int i = 0; i < addBuffersCount; i++)
                {
                    ref var buffer = ref _nativeAddOperationQueue.GetBag(i);
                    //todo: I don't like to iterate a constant number of buffer and skip the empty ones
                    while (buffer.IsEmpty() == false)
                    {
                        var componentsIndex = buffer.Dequeue<uint>();
                        var egid            = buffer.Dequeue<EGID>();
                        var reference       = buffer.Dequeue<EntityReference>();
                        var componentCounts = buffer.Dequeue<uint>();

                        Check.Assert(egid.groupID.isInvalid == false
                                   , "invalid group detected, are you using new ExclusiveGroupStruct() instead of new ExclusiveGroup()?");

                        ref var nativeOperation = ref _nativeAddOperations[componentsIndex];
#if DEBUG && !PROFILE_SVELTO
                        var entityDescriptorType = nativeOperation.entityDescriptorType;
                        CheckAddEntityID(egid, entityDescriptorType, nativeOperation.caller);
#endif

                        _entityLocator.SetReference(reference, egid);
                        //todo: I reckon is not necessary to carry the components array in the native operation, it's enough to know the descriptor type
                        //however I guess this can work only if the type is hashed, which could be done with the burst type hash
                        var dic = EntityFactory.BuildGroupedEntities(egid, _groupedEntityToAdd
                                                                   , nativeOperation.components, null
#if DEBUG && !PROFILE_SVELTO
                                                                   , entityDescriptorType
#endif
                        );

                        var init = new EntityInitializer(egid, dic, reference);

                        //only called if Init is called on the initialized (there is something to init)
                        while (componentCounts > 0)
                        {
                            componentCounts--;

                            var typeID = buffer.Dequeue<uint>();

                            IFiller componentBuilder = EntityComponentIDMap.GetBuilderFromID(typeID);
                            //after the typeID, I expect the serialized component
                            componentBuilder.FillFromByteArray(init, buffer);
                        }
                    }
                }
            }
        }

        void AllocateNativeOperations()
        {
            _nativeRemoveOperations = new FasterList<NativeOperationRemove>();
            _nativeSwapOperations   = new FasterList<NativeOperationSwap>();
            _nativeAddOperations    = new FasterList<NativeOperationBuild>();
        }

        FasterList<NativeOperationRemove> _nativeRemoveOperations;
        FasterList<NativeOperationSwap>   _nativeSwapOperations;
        FasterList<NativeOperationBuild>  _nativeAddOperations;

        readonly AtomicNativeBags _nativeAddOperationQueue;
        readonly AtomicNativeBags _nativeRemoveOperationQueue;
        readonly AtomicNativeBags _nativeSwapOperationQueue;
    }

    readonly struct DoubleEGID
    {
        internal readonly EGID from;
        internal readonly EGID to;

        public DoubleEGID(EGID from1, EGID to1)
        {
            from = from1;
            to   = to1;
        }
    }

    readonly struct NativeOperationBuild
    {
        internal readonly IComponentBuilder[] components;
        internal readonly Type                entityDescriptorType;
        internal readonly string              caller;

        public NativeOperationBuild
            (IComponentBuilder[] descriptorComponentsToBuild, Type entityDescriptorType, string caller)
        {
            this.entityDescriptorType = entityDescriptorType;
            components                = descriptorComponentsToBuild;
            this.caller               = caller;
        }
    }

    readonly struct NativeOperationRemove
    {
        internal readonly IComponentBuilder[] components;
        internal readonly Type                entityDescriptorType;
        internal readonly string              caller;

        public NativeOperationRemove
            (IComponentBuilder[] descriptorComponentsToRemove, Type entityDescriptorType, string caller)
        {
            this.caller               = caller;
            components                = descriptorComponentsToRemove;
            this.entityDescriptorType = entityDescriptorType;
        }
    }

    readonly struct NativeOperationSwap
    {
        internal readonly IComponentBuilder[] components;
        internal readonly Type                entityDescriptorType;
        internal readonly string              caller;

        public NativeOperationSwap
            (IComponentBuilder[] descriptorComponentsToSwap, Type entityDescriptorType, string caller)
        {
            this.caller               = caller;
            components                = descriptorComponentsToSwap;
            this.entityDescriptorType = entityDescriptorType;
        }
    }
}
#endif