#if UNITY_NATIVE //at the moment I am still considering NativeOperations useful only for Unity
using Svelto.ECS.DataStructures;

namespace Svelto.ECS.Native
{
    public readonly ref struct NativeEntityInitializer
    {
        readonly NativeBag        _unsafeBuffer;
        readonly UnsafeArrayIndex _index;
        readonly EntityReference  _reference;

        public NativeEntityInitializer(in NativeBag unsafeBuffer, UnsafeArrayIndex index, EntityReference reference)
        {
            _unsafeBuffer = unsafeBuffer;
            _index        = index;
            _reference    = reference;
        }

        public void Init<T>(in T component) where T : unmanaged, IEntityComponent
        {
            uint id = EntityComponentID<T>.ID.Data;

            _unsafeBuffer.AccessReserved<uint>(_index)++; //number of components added so far

            //Since NativeEntityInitializer is a ref struct, it guarantees that I am enqueueing components of the
            //last entity built
            _unsafeBuffer.Enqueue(id);
            _unsafeBuffer.Enqueue(component);
        }

        public EntityReference reference => _reference;
    }
}
#endif