namespace Svelto.ECS
{
    class GenericEntityStreamConsumerFactory : IEntityStreamConsumerFactory
    {
        public GenericEntityStreamConsumerFactory(DataStructures.WeakReference<EnginesRoot> weakReference)
        {
            _enginesRoot = weakReference;
        }

        public Consumer<T> GenerateConsumer<T>(string name, int capacity) where T : unmanaged, IEntityStruct
        {
            return _enginesRoot.Target.GenerateConsumer<T>(name, capacity);
        }

        readonly DataStructures.WeakReference<EnginesRoot> _enginesRoot;
    }
    
    public interface IEntityStreamConsumerFactory
    {
        Consumer<T> GenerateConsumer<T>(string name, int capacity) where T : unmanaged, IEntityStruct;
    }
}