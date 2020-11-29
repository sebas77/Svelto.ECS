#if UNITY_NATIVE
using Svelto.ECS.DataStructures;

namespace Svelto.ECS
{
    public readonly struct NativeEntityFactory
    {
        readonly AtomicNativeBags _addOperationQueue;
        readonly int             _index;

        internal NativeEntityFactory(AtomicNativeBags addOperationQueue, int index)
        {
            _index             = index;
            _addOperationQueue = addOperationQueue;
        }

        public NativeEntityComponentInitializer BuildEntity
            (uint eindex, BuildGroup BuildGroup, int threadIndex)
        {
            NativeBag unsafeBuffer = _addOperationQueue.GetBuffer(threadIndex + 1);

            unsafeBuffer.Enqueue(_index);
            unsafeBuffer.Enqueue(new EGID(eindex, BuildGroup));
            unsafeBuffer.ReserveEnqueue<uint>(out var index) = 0;

            return new NativeEntityComponentInitializer(unsafeBuffer, index);
        }
        
        public NativeEntityComponentInitializer BuildEntity(EGID egid, int threadIndex)
        {
            NativeBag unsafeBuffer = _addOperationQueue.GetBuffer(threadIndex + 1);

            unsafeBuffer.Enqueue(_index);
            unsafeBuffer.Enqueue(new EGID(egid.entityID, egid.groupID));
            unsafeBuffer.ReserveEnqueue<uint>(out var index) = 0;

            return new NativeEntityComponentInitializer(unsafeBuffer, index);
        }
    }
}
#endif