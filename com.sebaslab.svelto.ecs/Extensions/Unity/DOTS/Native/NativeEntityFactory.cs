#if UNITY_NATIVE
using Svelto.ECS.DataStructures;

namespace Svelto.ECS.Native
{
    public readonly struct NativeEntityFactory
    {
        internal NativeEntityFactory(AtomicNativeBags addOperationQueue, int index, EnginesRoot.LocatorMap entityLocator)
        {
            _index             = index;
            _addOperationQueue = addOperationQueue;
            _entityLocator     = entityLocator;
        }

        public NativeEntityInitializer BuildEntity
            (uint eindex, ExclusiveBuildGroup exclusiveBuildGroup, int threadIndex)
        {
            EntityReference reference = _entityLocator.ClaimReference();

            NativeBag unsafeBuffer = _addOperationQueue.GetBuffer(threadIndex + 1);

            unsafeBuffer.Enqueue(_index);
            unsafeBuffer.Enqueue(new EGID(eindex, exclusiveBuildGroup));
            unsafeBuffer.Enqueue(reference);
            unsafeBuffer.ReserveEnqueue<uint>(out var index) = 0;

            return new NativeEntityInitializer(unsafeBuffer, index, reference);
        }

        public NativeEntityInitializer BuildEntity(EGID egid, int threadIndex)
        {
            EntityReference reference = _entityLocator.ClaimReference();

            NativeBag unsafeBuffer = _addOperationQueue.GetBuffer(threadIndex + 1);

            unsafeBuffer.Enqueue(_index);
            unsafeBuffer.Enqueue(new EGID(egid.entityID, egid.groupID));
            unsafeBuffer.Enqueue(reference);
            unsafeBuffer.ReserveEnqueue<uint>(out var index) = 0;

            return new NativeEntityInitializer(unsafeBuffer, index, reference);
        }

        readonly EnginesRoot.LocatorMap  _entityLocator;
        readonly AtomicNativeBags        _addOperationQueue;
        readonly int                     _index;
    }
}
#endif