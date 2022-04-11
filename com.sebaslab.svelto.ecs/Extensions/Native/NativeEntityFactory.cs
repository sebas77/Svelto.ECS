#if UNITY_NATIVE
using Svelto.ECS.DataStructures;

namespace Svelto.ECS.Native
{
    public readonly struct NativeEntityFactory
    {
        internal NativeEntityFactory(AtomicNativeBags addOperationQueue, int index, EnginesRoot.EntityReferenceMap entityLocator)
        {
            _index             = index;
            _addOperationQueue = addOperationQueue;
            _entityLocator     = entityLocator;
        }

        public NativeEntityInitializer BuildEntity
            (uint eindex, ExclusiveBuildGroup exclusiveBuildGroup, int threadIndex)
        {
            EntityReference reference = _entityLocator.ClaimReference();
            NativeBag bagPerEntityPerThread = _addOperationQueue.GetBuffer(threadIndex + 1);

            bagPerEntityPerThread.Enqueue(_index); //each native ECS native operation is stored in an array, each request to perform a native operation in a queue. _index is the index of the operation in the array that will be dequeued later 
            bagPerEntityPerThread.Enqueue(new EGID(eindex, exclusiveBuildGroup));
            bagPerEntityPerThread.Enqueue(reference);
            
            //NativeEntityInitializer is quite a complex beast. It holds the starting values of the component set by the user. These components must be later dequeued and in order to know how many components
            //must be dequeued, a count must be used. The space to hold the count is then reserved in the queue and index will be used access the count later on through NativeEntityInitializer so it can increment it.
            //index is not the number of components of the entity, it's just the number of components that the user decide to initialise
            bagPerEntityPerThread.ReserveEnqueue<uint>(out var index) = 0;

            return new NativeEntityInitializer(bagPerEntityPerThread, index, reference);
        }

        public NativeEntityInitializer BuildEntity(EGID egid, int threadIndex)
        {
            return BuildEntity(egid.entityID, egid.groupID, threadIndex);
        }

        readonly EnginesRoot.EntityReferenceMap  _entityLocator;
        readonly AtomicNativeBags        _addOperationQueue;
        readonly int                     _index;
    }
}
#endif