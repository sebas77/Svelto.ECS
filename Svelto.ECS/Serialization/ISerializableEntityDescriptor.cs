using System;

namespace Svelto.ECS.Serialization
{
    public interface ISerializableEntityDescriptor : IEntityDescriptor
    {
        uint                         hash                { get; }
        ISerializableComponentBuilder[] entitiesToSerialize { get; }
        Type realType { get; }
    }
}