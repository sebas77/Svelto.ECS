#if UNITY_NATIVE
using Svelto.DataStructures;

namespace Svelto.ECS.Native
{
    public readonly struct NativeEntityRemove
    {
        readonly AtomicNativeBags _removeQueue;
        readonly int             _indexRemove;

        internal NativeEntityRemove(AtomicNativeBags EGIDsToRemove, int indexRemove)
        {
            _removeQueue = EGIDsToRemove;
            _indexRemove = indexRemove;
        }

        public void RemoveEntity(EGID egid, int threadIndex)
        {
            var simpleNativeBag = _removeQueue.GetBag(threadIndex);
            
            simpleNativeBag.Enqueue(_indexRemove);
            simpleNativeBag.Enqueue(egid);
        }
    }
}
#endif