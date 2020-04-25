#if UNITY_ECS
using Svelto.Common;
using Svelto.DataStructures;
using Svelto.ECS.DataStructures;
using Svelto.ECS.DataStructures.Unity;
using Unity.Jobs.LowLevel.Unsafe;

namespace Svelto.ECS
{
    public partial class EnginesRoot
    {
        //todo: I very likely don't need to create one for each native entity factory, the same can be reused
        readonly AtomicNativeBags _addOperationQueue =
            new AtomicNativeBags(Common.Allocator.Persistent, JobsUtility.MaxJobThreadCount + 1);

        readonly AtomicNativeBags _removeOperationQueue =
            new AtomicNativeBags(Common.Allocator.Persistent, JobsUtility.MaxJobThreadCount + 1);

        readonly AtomicNativeBags _swapOperationQueue =
            new AtomicNativeBags(Common.Allocator.Persistent, JobsUtility.MaxJobThreadCount + 1);

        NativeEntityRemove ProvideNativeEntityRemoveQueue<T>() where T : IEntityDescriptor, new()
        {
            //todo: remove operation array and store entity descriptor hash in the return value
            //todo I maybe able to provide a  _nativeSwap.SwapEntity<entityDescriptor> 
            _nativeRemoveOperations.Add(
                new NativeOperationRemove(EntityDescriptorTemplate<T>.descriptor.componentsToBuild));

            return new NativeEntityRemove(_removeOperationQueue, _nativeRemoveOperations.count - 1);
        }
        
        NativeEntitySwap ProvideNativeEntitySwapQueue<T>() where T : IEntityDescriptor, new()
        {
            //todo: remove operation array and store entity descriptor hash in the return value
            _nativeSwapOperations.Add(
                new NativeOperationSwap(EntityDescriptorTemplate<T>.descriptor.componentsToBuild));

            return new NativeEntitySwap(_swapOperationQueue, _nativeSwapOperations.count - 1);
        }

        NativeEntityFactory ProvideNativeEntityFactoryQueue<T>() where T : IEntityDescriptor, new()
        {
            //todo: remove operation array and store entity descriptor hash in the return value
            _nativeAddOperations.Add(
                new NativeOperationBuild(EntityDescriptorTemplate<T>.descriptor.componentsToBuild));

            return new NativeEntityFactory(_addOperationQueue, _nativeAddOperations.count - 1);
        }

        void NativeOperationSubmission(in PlatformProfiler profiler)
        {
            using (profiler.Sample("Native Remove/Swap Operations"))
            {
                for (int i = 0; i < _removeOperationQueue.count; i++)
                {
                    ref var buffer = ref _removeOperationQueue.GetBuffer(i);

                    while (buffer.IsEmpty() == false)
                    {
                        var componentsIndex = buffer.Dequeue<uint>();
                        var entityEGID = buffer.Dequeue<EGID>();
                        CheckRemoveEntityID(entityEGID); 
                        QueueEntitySubmitOperation(new EntitySubmitOperation(
                                                       EntitySubmitOperationType.Remove, entityEGID, entityEGID
                                                     , _nativeRemoveOperations[componentsIndex].entityComponents));
                    }
                }

                for (int i = 0; i < _swapOperationQueue.count; i++)
                {
                    ref var buffer = ref _swapOperationQueue.GetBuffer(i);

                    while (buffer.IsEmpty() == false)
                    {
                        var     componentsIndex = buffer.Dequeue<uint>();
                        var entityEGID      = buffer.Dequeue<DoubleEGID>();
                        
                        CheckRemoveEntityID(entityEGID.@from);
                        CheckAddEntityID(entityEGID.to); 
                        
                        QueueEntitySubmitOperation(new EntitySubmitOperation(
                                                       EntitySubmitOperationType.Swap, entityEGID.@from, entityEGID.to
                                                     , _nativeSwapOperations[componentsIndex].entityComponents));
                    }
                }
            }
            
            using (profiler.Sample("Native Add Operations"))
            {
                for (int i = 0; i < _addOperationQueue.count; i++)
                {
                    ref var buffer = ref _addOperationQueue.GetBuffer(i);
                    
                    while (buffer.IsEmpty() == false)
                    {
                        var componentsIndex = buffer.Dequeue<uint>();
                        var egid            = buffer.Dequeue<EGID>();
                        var componentCounts = buffer.Dequeue<uint>();
                        
                        EntityComponentInitializer init =
                            BuildEntity(egid, _nativeAddOperations[componentsIndex].components);

                        while (componentCounts > 0)
                        {
                            componentCounts--;

                            var typeID = buffer.Dequeue<uint>();

                            IFiller entityBuilder = EntityComponentIDMap.GetTypeFromID(typeID);

                            //after the typeID, I expect the serialized component
                            entityBuilder.FillFromByteArray(init, buffer);
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

    public readonly struct NativeEntityRemove
    {
        readonly AtomicNativeBags _removeQueue;
        readonly uint              _indexRemove;

        internal NativeEntityRemove(AtomicNativeBags EGIDsToRemove, uint indexRemove)
        {
            _removeQueue = EGIDsToRemove;
            _indexRemove = indexRemove;
        }

        public void RemoveEntity(EGID egid, int threadIndex)
        {
            var simpleNativeBag = _removeQueue.GetBuffer(threadIndex);
            
            simpleNativeBag.Enqueue(_indexRemove);
            simpleNativeBag.Enqueue(egid);
        }
    }
    
    public readonly struct NativeEntitySwap
    {
        readonly AtomicNativeBags _swapQueue;
        readonly uint              _indexSwap;

        internal NativeEntitySwap(AtomicNativeBags EGIDsToSwap, uint indexSwap)
        {
            _swapQueue   = EGIDsToSwap;
            _indexSwap   = indexSwap;
        }

        public void SwapEntity(EGID from, EGID to, int threadIndex)
        {
            var simpleNativeBag = _swapQueue.GetBuffer(threadIndex);
            simpleNativeBag.Enqueue(_indexSwap);
            simpleNativeBag.Enqueue(new DoubleEGID(from, to));
        }

        public void SwapEntity(EGID from, ExclusiveGroupStruct to, int threadIndex)
        {
            var simpleNativeBag = _swapQueue.GetBuffer(threadIndex);
            simpleNativeBag.Enqueue(_indexSwap);
            simpleNativeBag.Enqueue(new DoubleEGID(from, new EGID(from.entityID, to)));
        }
    }

    public readonly struct NativeEntityFactory
    {
        readonly AtomicNativeBags _addOperationQueue;
        readonly uint              _index;

        internal NativeEntityFactory(AtomicNativeBags addOperationQueue, uint index)
        {
            _index             = index;
            _addOperationQueue = addOperationQueue;
        }

        public NativeEntityComponentInitializer BuildEntity
            (uint eindex, ExclusiveGroupStruct buildGroup, int threadIndex)
        {
            NativeBag unsafeBuffer = _addOperationQueue.GetBuffer(threadIndex + 1);

            unsafeBuffer.Enqueue(_index);
            unsafeBuffer.Enqueue(new EGID(eindex, buildGroup));
            unsafeBuffer.ReserveEnqueue<uint>(out var index) = 0;

            return new NativeEntityComponentInitializer(unsafeBuffer, index);
        }
    }

    public readonly ref struct NativeEntityComponentInitializer
    {
        readonly NativeBag  _unsafeBuffer;
        readonly UnsafeArrayIndex _index;

        public NativeEntityComponentInitializer(in NativeBag unsafeBuffer, UnsafeArrayIndex index)
        {
            _unsafeBuffer = unsafeBuffer;
            _index        = index;
        }

        public void Init<T>(in T component) where T : unmanaged, IEntityComponent
        {
            uint id = EntityComponentID<T>.ID.Data;

            _unsafeBuffer.AccessReserved<uint>(_index)++;

            _unsafeBuffer.Enqueue(id);
            _unsafeBuffer.Enqueue(component);
        }
    }

    struct NativeOperationBuild
    {
        internal readonly IComponentBuilder[] components;

        public NativeOperationBuild(IComponentBuilder[] descriptorEntityComponentsToBuild)
        {
            components = descriptorEntityComponentsToBuild;
        }
    }

    readonly struct NativeOperationRemove
    {
        internal readonly IComponentBuilder[] entityComponents;

        public NativeOperationRemove(IComponentBuilder[] descriptorEntitiesToBuild)
        {
            entityComponents = descriptorEntitiesToBuild;
        }
    }

    readonly struct NativeOperationSwap
    {
        internal readonly IComponentBuilder[] entityComponents;

        public NativeOperationSwap(IComponentBuilder[] descriptorEntitiesToBuild)
        {
            entityComponents = descriptorEntitiesToBuild;
        }
    }
}
#endif