using System;
using Svelto.DataStructures;

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
    
    class EntitiesStream : IDisposable
    {
        internal Consumer<T> GenerateConsumer<T>(string name, uint capacity) where T : unmanaged, IEntityStruct
        {
            if (_streams.ContainsKey(TypeRefWrapper<T>.wrapper) == false) _streams[TypeRefWrapper<T>.wrapper] = new EntityStream<T>();

            return (_streams[TypeRefWrapper<T>.wrapper] as EntityStream<T>).GenerateConsumer(name, capacity);
        }

        public Consumer<T> GenerateConsumer<T>(ExclusiveGroup group, string name, uint capacity)
            where T : unmanaged, IEntityStruct
        {
            if (_streams.ContainsKey(TypeRefWrapper<T>.wrapper) == false) _streams[TypeRefWrapper<T>.wrapper] = new EntityStream<T>();

            return (_streams[TypeRefWrapper<T>.wrapper] as EntityStream<T>).GenerateConsumer(group, name, capacity);
        }

        internal void PublishEntity<T>(ref T entity, EGID egid) where T : unmanaged, IEntityStruct
        {
            if (_streams.TryGetValue(TypeRefWrapper<T>.wrapper, out var typeSafeStream))
                (typeSafeStream as EntityStream<T>).PublishEntity(ref entity, egid);
            else
                Console.LogDebug("No Consumers are waiting for this entity to change ", typeof(T));
        }

        readonly ThreadSafeDictionary<RefWrapper<Type>, ITypeSafeStream> _streams =
            new ThreadSafeDictionary<RefWrapper<Type>, ITypeSafeStream>();

        public void Dispose()
        {
            _streams.Clear();
        }
    }

    interface ITypeSafeStream
    {}

    class EntityStream<T> : ITypeSafeStream where T : unmanaged, IEntityStruct
    {
        public void PublishEntity(ref T entity, EGID egid)
        {
            for (int i = 0; i < _consumers.Count; i++)
            {
                if (_consumers[i]._hasGroup)
                {
                    if (egid.groupID == _consumers[i]._group)
                    {
                        _consumers[i].Enqueue(entity, egid);
                    }
                }
                else
                {
                    _consumers[i].Enqueue(entity, egid);
                }
            }
        }

        public Consumer<T> GenerateConsumer(string name, uint capacity)
        {
            var consumer = new Consumer<T>(name, capacity, this);

            _consumers.Add(consumer);

            return consumer;
        }

        public Consumer<T> GenerateConsumer(ExclusiveGroup group, string name, uint capacity)
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

    public struct Consumer<T> : IDisposable where T : unmanaged, IEntityStruct
    {
        internal Consumer(string name, uint capacity, EntityStream<T> stream):this()
        {
#if DEBUG && !PROFILER
            _name = name;
#endif
            _ringBuffer = new RingBuffer<ValueTuple<T, EGID>>((int) capacity,
#if DEBUG && !PROFILER
                _name
#else
                string.Empty
#endif
                );

            _stream = stream;
        }

        internal Consumer(ExclusiveGroup group, string name, uint capacity, EntityStream<T> stream) : this(name,
            capacity, stream)
        {
            _group = group;
            _hasGroup = true;
        }

        internal void Enqueue(in T entity, in EGID egid)
        {
            _ringBuffer.Enqueue((entity, egid));
        }

        public bool TryDequeue(out T entity)
        {
            var tryDequeue = _ringBuffer.TryDequeue(out var values);

            entity = values.Item1;

            return tryDequeue;
        }

        public bool TryDequeue(out T entity, out EGID id)
        {
            var tryDequeue = _ringBuffer.TryDequeue(out var values);

            entity = values.Item1;
            id = values.Item2;

            return tryDequeue;
        }
        public void Flush() { _ringBuffer.Reset(); }
        public void Dispose() { _stream.RemoveConsumer(this); }
        public uint Count() { return (uint) _ringBuffer.Count; }

        readonly          RingBuffer<ValueTuple<T, EGID>>   _ringBuffer;
        readonly          EntityStream<T> _stream;

        internal readonly ExclusiveGroup  _group;
        internal readonly bool            _hasGroup;

#if DEBUG && !PROFILER
        readonly string _name;
#endif
    }
}
