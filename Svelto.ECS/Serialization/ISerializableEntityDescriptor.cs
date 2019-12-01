namespace Svelto.ECS.Serialization
{
    public interface ISerializableEntityDescriptor : IEntityDescriptor
    {
        uint                         hash                { get; }
        ISerializableEntityBuilder[] entitiesToSerialize { get; }
        
        void CopySerializedEntityStructs(in EntityStructInitializer sourceInitializer, in EntityStructInitializer destinationInitializer);
    }
}