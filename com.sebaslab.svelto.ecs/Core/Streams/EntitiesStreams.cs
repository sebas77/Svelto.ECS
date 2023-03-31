using System;
using Svelto.DataStructures;
using Svelto.ECS.Internal;

namespace Svelto.ECS
{
    /// <summary>
    ///     I eventually realised that, with the ECS design, no form of engines (systems) communication other
    ///     than polling entity components is effective.
    ///     The only purpose of this publisher/consumer model is to let two enginesroots communicate with each other
    ///     through a thread safe ring buffer.
    ///     The engines root A publishes entities.
    ///     The engines root B can consume those entities at any time, as they will be a copy of the original
    ///     entities and won't point directly to the database of the engines root A
    /// </summary>
    struct EntitiesStreams : IDisposable
    {
        internal Consumer<T> GenerateConsumer<T>(string name, uint capacity)
            where T : unmanaged, _IInternalEntityComponent
        {
            var componentId = ComponentTypeID<T>.id;
            
            if (_streams.ContainsKey(componentId) == false)
                _streams[componentId] = new EntityStream<T>();

            return (_streams[componentId] as EntityStream<T>).GenerateConsumer(name, capacity);
        }

        public Consumer<T> GenerateConsumer<T>(ExclusiveGroupStruct group, string name, uint capacity)
            where T : unmanaged, _IInternalEntityComponent
        {
            var componentId = ComponentTypeID<T>.id;
            
            if (_streams.ContainsKey(componentId) == false)
                _streams[componentId] = new EntityStream<T>();

            var typeSafeStream = (EntityStream<T>) _streams[componentId];
            return typeSafeStream.GenerateConsumer(group, name, capacity);
        }

        internal void PublishEntity<T>(ref T entity, EGID egid) where T : unmanaged, _IInternalEntityComponent
        {
            if (_streams.TryGetValue(ComponentTypeID<T>.id, out var typeSafeStream))
                (typeSafeStream as EntityStream<T>).PublishEntity(ref entity, egid);
            else
                Console.LogDebug($"No Consumers are waiting for this entity to change {typeof(T)}");
        }

        public void Dispose()
        {
            foreach (var stream in _streams)
                stream.value.Dispose();
        }

        public static EntitiesStreams Create()
        {
            var stream = new EntitiesStreams();
            stream._streams = FasterDictionary<ComponentID, ITypeSafeStream>.Construct();

            return stream;
        }

        FasterDictionary<ComponentID, ITypeSafeStream> _streams;
    }
}