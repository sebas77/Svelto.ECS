#if UNITY_BURST
using Svelto.ECS.DataStructures;
using Svelto.ECS.DataStructures.Unity;

namespace Svelto.ECS
{
    public readonly struct NativeEntityFactory
    {
        readonly AtomicNativeBags _addOperationQueue;
        readonly uint             _index;

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