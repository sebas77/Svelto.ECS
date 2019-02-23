namespace Svelto.ECS
{
    public partial class EnginesRoot
    {
        class GenericEntitiesStream : IEntitiesStream
        {
            public GenericEntitiesStream(DataStructures.WeakReference<EnginesRoot> weakReference)
            {
                _weakEngine = weakReference;
            }
            
            public Consumer<T> GenerateConsumer<T>(int capacity) where T : unmanaged, IEntityStruct
            {
                return _weakEngine.Target._entitiesStream.GenerateConsumer<T>(capacity);
            }

            public void PublishEntity<T>(EGID id) where T : unmanaged, IEntityStruct
            {
                _weakEngine.Target._entitiesStream.PublishEntity<T>(id);
            }
            
            readonly DataStructures.WeakReference<EnginesRoot> _weakEngine;
        }
    }
}