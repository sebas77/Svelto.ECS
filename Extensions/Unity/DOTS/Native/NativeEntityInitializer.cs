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

            _unsafeBuffer.AccessReserved<uint>(_index)++;

            _unsafeBuffer.Enqueue(id);
            _unsafeBuffer.Enqueue(component);
        }

        public EntityReference reference => _reference;
    }
}