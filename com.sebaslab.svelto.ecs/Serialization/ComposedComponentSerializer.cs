using System;
using Svelto.ECS.Internal;

namespace Svelto.ECS.Serialization
{
    public class ComposedComponentSerializer<T, X, Y> : IComponentSerializer<T>
        where T : unmanaged, _IInternalEntityComponent where X : class, IComponentSerializer<T>, new()
        where Y : class, IComponentSerializer<T>, new()
    {
        public ComposedComponentSerializer()
        {
            _serializers = new IComponentSerializer<T>[2];
            _serializers[0] = new X();
            _serializers[1] = new Y();
        }

        public bool Serialize(in T value, ISerializationData serializationData)
        {
            foreach (IComponentSerializer<T> s in _serializers)
            {
                serializationData.data.IncrementCountBy((uint)s.size);
                if (s.SerializeSafe(value, serializationData))
                    return true;
            }

            throw new Exception($"ComposedComponentSerializer for {typeof(T)} did not serialize any data!");
        }

        public bool Deserialize(ref T value, ISerializationData serializationData)
        {
            foreach (IComponentSerializer<T> s in _serializers)
            {
                if (s.DeserializeSafe(ref value, serializationData))
                    return true;
            }

            throw new Exception($"ComposedComponentSerializer for {typeof(T)} did not deserialize any data!");
        }

        public   uint                      size => 0;
        readonly IComponentSerializer<T>[] _serializers;
    }
}