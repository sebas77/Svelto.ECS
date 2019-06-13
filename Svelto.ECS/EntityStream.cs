using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Svelto.DataStructures;
using Svelto.ECS.Internal;

namespace Svelto.ECS
{
    /// <summary>
    /// Do not use this class in place of a normal polling.
    /// I eventually realised than in ECS no form of communication other than polling entity components can exist.
    /// Using groups, you can have always an optimal set of entity components to poll, so EntityStreams must be used
    /// only if:
    /// - you want to polling engine to be able to track all the entity changes happening in between polls and not
    /// just the current state
    /// - you want a thread-safe way to read entity states, which includes all the state changes and not the last
    /// one only
    /// - you want to communicate between EnginesRoots
    /// </summary>
    class EntitiesStream
    {
        internal Consumer<T> GenerateConsumer<T>(string name, int capacity) where T : unmanaged, IEntityStruct
        {
            if (_streams.ContainsKey(typeof(T)) == false) _streams[typeof(T)] = new EntityStream<T>();

            return (_streams[typeof(T)] as EntityStream<T>).GenerateConsumer(name, capacity);
        }
        
        public Consumer<T> GenerateConsumer<T>(ExclusiveGroup @group, string name, int capacity) where T : unmanaged, IEntityStruct
        {
            if (_streams.ContainsKey(typeof(T)) == false) _streams[typeof(T)] = new EntityStream<T>();

            return (_streams[typeof(T)] as EntityStream<T>).GenerateConsumer( @group, name, capacity);
        }

        internal void PublishEntity<T>(ref T entity) where T : unmanaged, IEntityStruct
        {
            if (_streams.TryGetValue(typeof(T), out var typeSafeStream))
                (typeSafeStream as EntityStream<T>).PublishEntity(ref entity);
            else
                Console.LogWarningDebug("No Consumers are waiting for this entity to change ", typeof(T));
        }

        readonly ConcurrentDictionary<Type, ITypeSafeStream> _streams =
            new ConcurrentDictionary<Type, ITypeSafeStream>();
    }

    interface ITypeSafeStream
    {}

    class EntityStream<T>:ITypeSafeStream where T:unmanaged, IEntityStruct
    {
        public void PublishEntity(ref T entity)
        {
            for (int i = 0; i < _consumers.Count; i++)
            {
                if (_consumers[i]._hasGroup)
                {
                    if (EntityBuilder<T>.HAS_EGID && (entity as INeedEGID).ID.groupID == _consumers[i]._group)
                    {
                        _consumers[i].Enqueue(ref entity);
                    }
                }
                else
                    _consumers[i].Enqueue(ref entity);
            }
        }

        public Consumer<T> GenerateConsumer(string name, int capacity)
        {
            var consumer = new Consumer<T>(name, capacity, this);
            
            _consumers.Add(consumer);
            
            return consumer;
        }
        
        public Consumer<T> GenerateConsumer(ExclusiveGroup @group, string name, int capacity)
        {
            var consumer = new Consumer<T>(group, name, capacity, this);
            
            _consumers.Add(consumer);
            
            return consumer;
        }

        public void RemoveConsumer(Consumer<T> consumer)
        {
            _consumers.UnorderedRemove(consumer);
        }

        readonly FasterListThreadSafe<Consumer<T>> _consumers = new FasterListThreadSafe<Consumer<T>>();
    }

    public struct Consumer<T>: IDisposable where T:unmanaged, IEntityStruct
    {
        internal Consumer(string name, int capacity, EntityStream<T> stream): this()
        {
            _ringBuffer = new RingBuffer<T>(capacity);
            _name = name;
            _stream = stream;
        }
        
        internal Consumer(ExclusiveGroup @group, string name, int capacity, EntityStream<T> stream):this(name, capacity, stream)
        {
            _group = group;
            _hasGroup = true;
        }

        internal void Enqueue(ref T entity)
        {
            _ringBuffer.Enqueue(ref entity, _name);
        }
        
        public bool TryDequeue(out T entity) { return _ringBuffer.TryDequeue(out entity, _name); }
        public void Flush() { _ringBuffer.Reset(); }
        public void Dispose() { _stream.RemoveConsumer(this); }

        readonly RingBuffer<T>   _ringBuffer;
        readonly EntityStream<T> _stream;
        readonly string          _name;
        internal readonly ExclusiveGroup  _group;
        internal readonly bool _hasGroup;
    }
}