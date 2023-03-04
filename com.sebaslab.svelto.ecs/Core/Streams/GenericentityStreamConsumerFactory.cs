using System.Runtime.CompilerServices;
using Svelto.DataStructures;
using Svelto.ECS.Internal;

namespace Svelto.ECS
{
    class GenericEntityStreamConsumerFactory : IEntityStreamConsumerFactory
    {
        public GenericEntityStreamConsumerFactory(EnginesRoot weakReference)
        {
            _enginesRoot = new WeakReference<EnginesRoot>(weakReference);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Consumer<T> GenerateConsumer<T>(string name, uint capacity)
            where T : unmanaged, _IInternalEntityComponent
        {
            return _enginesRoot.Target.GenerateConsumer<T>(name, capacity);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Consumer<T> GenerateConsumer<T>(ExclusiveGroupStruct @group, string name, uint capacity)
            where T : unmanaged, _IInternalEntityComponent
        {
            return _enginesRoot.Target.GenerateConsumer<T>(group, name, capacity);
        }

//enginesRoot is a weakreference because GenericEntityStreamConsumerFactory can be injected inside
//engines of other enginesRoot
        readonly WeakReference<EnginesRoot> _enginesRoot;
    }

    public interface IEntityStreamConsumerFactory
    {
        Consumer<T> GenerateConsumer<T>(string name, uint capacity) where T : unmanaged, _IInternalEntityComponent;

        Consumer<T> GenerateConsumer<T>(ExclusiveGroupStruct @group, string name, uint capacity)
            where T : unmanaged, _IInternalEntityComponent;
    }
}