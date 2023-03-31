#if UNITY_NATIVE //at the moment I am still considering NativeOperations useful only for Unity
using System.Runtime.CompilerServices;
using Svelto.DataStructures;

namespace Svelto.ECS.Native
{
    public readonly ref struct NativeEntityInitializer
    {
        readonly NativeBag _unsafeBuffer;
        readonly UnsafeArrayIndex _componentsToInitializeCounterRef;
        readonly EntityReference _reference;

        public NativeEntityInitializer(in NativeBag unsafeBuffer, UnsafeArrayIndex componentsToInitializeCounterRef, EntityReference reference)
        {
            _unsafeBuffer = unsafeBuffer;
            _componentsToInitializeCounterRef = componentsToInitializeCounterRef;
            _reference = reference;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref T Init<T>(in T component) where T : unmanaged, IEntityComponent
        {
            uint componentID = ComponentTypeID<T>.id;

            _unsafeBuffer.AccessReserved<uint>(_componentsToInitializeCounterRef)++; //increase the number of components that have been initialised by the user

            //Since NativeEntityInitializer is a ref struct, it guarantees that I am enqueueing components of the
            //last entity built
            _unsafeBuffer.Enqueue(componentID); //to know what component it's being stored
            _unsafeBuffer.ReserveEnqueue<T>(out var index) = component;

            return ref _unsafeBuffer.AccessReserved<T>(index);
        }

        public EntityReference reference => _reference;
    }
}
#endif