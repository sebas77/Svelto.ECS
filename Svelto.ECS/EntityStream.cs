using System;
using System.Runtime.InteropServices;
using Svelto.DataStructures;

namespace Svelto.ECS
{
    /// <summary>
    /// I eventually realised that, with the ECS design, no form of communication other than polling entity components can exist.
    /// Using groups, you can have always an optimal set of entity components to poll. However EntityStreams  
    /// can be useful if:
    /// - you need to react on seldom entity changes, usually due to user events
    /// - you want engines to be able to track entity changes
    /// - you want a thread-safe way to read entity states, which includes all the state changes and not the last
    /// one only
    /// - you want to communicate between EnginesRoots
    /// </summary>
    class EntitiesStream : IDisposable
    {
        internal Consumer<T> GenerateConsumer<T>(string name, uint capacity)
            where T : unmanaged, IEntityComponent
        {
            if (_streams.ContainsKey(TypeRefWrapper<T>.wrapper) == false)
                _streams[TypeRefWrapper<T>.wrapper] = new EntityStream<T>();

            return (_streams[TypeRefWrapper<T>.wrapper] as EntityStream<T>).GenerateConsumer(name, capacity);
        }

        public Consumer<T> GenerateConsumer<T>(ExclusiveGroupStruct group, string name, uint capacity)
            where T : unmanaged, IEntityComponent
        {
            if (_streams.ContainsKey(TypeRefWrapper<T>.wrapper) == false)
                _streams[TypeRefWrapper<T>.wrapper] = new EntityStream<T>();

            EntityStream<T> typeSafeStream = (EntityStream<T>) _streams[TypeRefWrapper<T>.wrapper];
            return typeSafeStream.GenerateConsumer(group, name, capacity);
        }

        internal void PublishEntity<T>(ref T entity, EGID egid) where T : unmanaged, IEntityComponent
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
    { }

    public class EntityStream<T> : ITypeSafeStream where T : unmanaged, IEntityComponent
    {
        ~EntityStream()
        {
            for (int i = 0; i < _consumers.Count; i++)
                _consumers[i].Free();
        }
        
        internal EntityStream()
        {
            _consumers = new ThreadSafeFasterList<Consumer<T>>();
        }
        
        internal void PublishEntity(ref T entity, EGID egid)
        {
            for (int i = 0; i < _consumers.Count; i++)
            {
                unsafe
                {
                    if (*(bool *)_consumers[i].mustBeDisposed)
                    {
                        _consumers[i].Free();
                        _consumers.UnorderedRemoveAt(i);
                        --i;
                        continue;
                    }
                
                    if (_consumers[i].hasGroup)
                    {
                        if (egid.groupID == _consumers[i].@group)
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
        }

        internal Consumer<T> GenerateConsumer(string name, uint capacity)
        {
            var consumer = new Consumer<T>(name, capacity);

            _consumers.Add(consumer);

            return consumer;
        }

        internal Consumer<T> GenerateConsumer(ExclusiveGroupStruct group, string name, uint capacity)
        {
            var consumer = new Consumer<T>(group, name, capacity);

            _consumers.Add(consumer);

            return consumer;
        }

        readonly ThreadSafeFasterList<Consumer<T>> _consumers;
    }

    public struct Consumer<T> :IDisposable where T : unmanaged, IEntityComponent
    {
        internal Consumer(string name, uint capacity) : this()
        {
            unsafe
            {
#if DEBUG && !PROFILE_SVELTO
                _name = name;
#endif
                _ringBuffer = new RingBuffer<ValueTuple<T, EGID>>((int) capacity,
#if DEBUG && !PROFILE_SVELTO
                    _name
#else
                string.Empty
#endif
                );
                mustBeDisposed = Marshal.AllocHGlobal(sizeof(bool));
                *((bool*) mustBeDisposed) = false;
            }
        }

        internal Consumer(ExclusiveGroupStruct group, string name, uint capacity) : this(name, capacity)
        {
            this.@group = @group;
            hasGroup = true;
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

        public void Flush()
        {
            _ringBuffer.Reset();
        }

        public void Dispose()
        {
            unsafe
            {
                *(bool *)mustBeDisposed = true;
            }
        }

        public uint Count()
        {
            return (uint) _ringBuffer.Count;
        }
        
        public void Free()
        {
            Marshal.FreeHGlobal(mustBeDisposed);
        }

        readonly RingBuffer<ValueTuple<T, EGID>> _ringBuffer;

        internal readonly ExclusiveGroupStruct @group;
        internal readonly bool           hasGroup;
        internal          IntPtr         mustBeDisposed;

#if DEBUG && !PROFILE_SVELTO
        readonly string _name;
#endif
    }
}