#if UNITY_BURST
using Svelto.ECS.DataStructures.Unity;

namespace Svelto.ECS
{
    public readonly struct NativeEntitySwap
    {
        readonly AtomicNativeBags _swapQueue;
        readonly uint             _indexSwap;

        internal NativeEntitySwap(AtomicNativeBags EGIDsToSwap, uint indexSwap)
        {
            _swapQueue = EGIDsToSwap;
            _indexSwap = indexSwap;
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
}
#endif