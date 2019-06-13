namespace Svelto.ECS
{
    class GenericentityStreamConsumerFactory : IEntityStreamConsumerFactory
    {
        public GenericentityStreamConsumerFactory(DataStructures.WeakReference<EnginesRoot> weakReference)
        {
            _enginesRoot = weakReference;
        }

        public Consumer<T> GenerateConsumer<T>(string name, int capacity) where T : unmanaged, IEntityStruct
        {
            return _enginesRoot.Target.GenerateConsumer<T>(name, capacity);
        }

        public Consumer<T> GenerateConsumer<T>(ExclusiveGroup group, string name, int capacity) where T : unmanaged, IEntityStruct
        {
            return _enginesRoot.Target.GenerateConsumer<T>(group, name, capacity);
        }

        readonly DataStructures.WeakReference<EnginesRoot> _enginesRoot;
    }
    
    public interface IEntityStreamConsumerFactory
    {
        Consumer<T> GenerateConsumer<T>(string name, int capacity) where T : unmanaged, IEntityStruct;
        Consumer<T> GenerateConsumer<T>(ExclusiveGroup group, string name, int capacity) 
            where T : unmanaged, IEntityStruct;
    }
}