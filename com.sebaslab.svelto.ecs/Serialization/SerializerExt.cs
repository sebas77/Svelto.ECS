using Svelto.ECS.Internal;

namespace Svelto.ECS.Serialization
{
    public static class SerializerExt
    {
        public static bool SerializeSafe<T>
            (this IComponentSerializer<T> componentSerializer, in T value, ISerializationData serializationData)
            where T : unmanaged, _IInternalEntityComponent
        {
#if DEBUG && !PROFILE_SVELTO
            uint posBefore = serializationData.dataPos;
#endif
            bool res = componentSerializer.Serialize(value, serializationData);
#if DEBUG && !PROFILE_SVELTO
            // size == 0 is a special case when we don't know the size in advance
            if (componentSerializer.size != 0 && serializationData.dataPos != posBefore + componentSerializer.size)
            {
                throw new System.IndexOutOfRangeException(
                    $"Size mismatch when serializing {typeof(T).FullName} using {componentSerializer.GetType().FullName}, "
                  + $"expected offset {posBefore + componentSerializer.size}, got {serializationData.dataPos}");
            }
#endif
            return res;
        }

        public static bool DeserializeSafe<T>
            (this IComponentSerializer<T> componentSerializer, ref T value, ISerializationData serializationData)
            where T : unmanaged, _IInternalEntityComponent
        {
#if DEBUG && !PROFILE_SVELTO
            uint posBefore = serializationData.dataPos;
#endif
            bool res = componentSerializer.Deserialize(ref value, serializationData);
#if DEBUG && !PROFILE_SVELTO
            if (componentSerializer.size != 0 && serializationData.dataPos != posBefore + componentSerializer.size)
            {
                throw new System.IndexOutOfRangeException(
                    $"Size mismatch when deserializing {typeof(T).FullName} using {componentSerializer.GetType().FullName}, "
                  + $"expected offset {posBefore + componentSerializer.size}, got {serializationData.dataPos}");
            }
#endif
            return res;
        }
    }
}