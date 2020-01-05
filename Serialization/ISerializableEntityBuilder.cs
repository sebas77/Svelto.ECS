using Svelto.ECS.Internal;

namespace Svelto.ECS.Serialization
{
    public interface ISerializableEntityBuilder : IEntityBuilder
    {
        void Serialize(uint id, ITypeSafeDictionary dictionary, ISerializationData serializationData, SerializationType serializationType);

        void Deserialize(uint id, ITypeSafeDictionary dictionary, ISerializationData serializationData, SerializationType serializationType);

        void Deserialize(ISerializationData serializationData, in EntityStructInitializer initializer, SerializationType serializationType);
        
        void CopySerializedEntityStructs(in EntityStructInitializer sourceInitializer, in EntityStructInitializer destinationInitializer, SerializationType serializationType);
    }
}
