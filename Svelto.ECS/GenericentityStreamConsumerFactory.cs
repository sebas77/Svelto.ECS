using Svelto.DataStructures;

namespace Svelto.ECS
{
    class GenericEntityStreamConsumerFactory : IEntityStreamConsumerFactory
    {
        public GenericEntityStreamConsumerFactory(EnginesRoot weakReference)
        {
            _enginesRoot = new WeakReference<EnginesRoot>(weakReference);
        }

        public Consumer<T> GenerateConsumer<T>(string name, uint capacity) where T : unmanaged, IEntityStruct
        {
            return _enginesRoot.Target.GenerateConsumer<T>(name, capacity);
        }

        public Consumer<T> GenerateConsumer<T>(ExclusiveGroup group, string name, uint capacity) where T : unmanaged, IEntityStruct
        {
            return _enginesRoot.Target.GenerateConsumer<T>(group, name, capacity);
        }

//enginesRoot is a weakreference because GenericEntityStreamConsumerFactory can be injected inside
//engines of other enginesRoot
        readonly WeakReference<EnginesRoot> _enginesRoot;
    }
    
    public interface IEntityStreamConsumerFactory
    {
        Consumer<T> GenerateConsumer<T>(string name, uint capacity) where T : unmanaged, IEntityStruct;
        Consumer<T> GenerateConsumer<T>(ExclusiveGroup group, string name, uint capacity) 
            where T : unmanaged, IEntityStruct;
    }
}