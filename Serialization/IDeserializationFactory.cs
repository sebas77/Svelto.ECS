namespace Svelto.ECS.Serialization
{
    public interface IDeserializationFactory
    {
        EntityComponentInitializer BuildDeserializedEntity
        (EGID egid, ISerializationData serializationData, ISerializableEntityDescriptor entityDescriptor
       , int serializationType, IEntitySerialization entitySerialization, IEntityFactory factory
       , bool enginesRootIsDeserializationOnly);
    }
}
