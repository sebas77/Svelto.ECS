namespace Svelto.ECS.Serialization
{
    public interface ISerializableEntityDescriptor : IEntityDescriptor
    {
        uint                         hash                { get; }
        ISerializableComponentBuilder[] entitiesToSerialize { get; }
    }
}