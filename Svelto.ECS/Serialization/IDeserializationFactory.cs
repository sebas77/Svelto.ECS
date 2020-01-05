namespace Svelto.ECS.Serialization
{
    public interface IDeserializationFactory
    {
        EntityStructInitializer BuildDeserializedEntity(EGID egid, ISerializationData serializationData,
            ISerializableEntityDescriptor entityDescriptor, SerializationType serializationType,
            IEntitySerialization entitySerialization);
    }
}
