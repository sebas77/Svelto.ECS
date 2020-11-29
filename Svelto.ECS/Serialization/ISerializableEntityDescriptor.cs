using System;

namespace Svelto.ECS.Serialization
{
    public interface ISerializableEntityDescriptor : IDynamicEntityDescriptor
    {
        uint                            hash                { get; }
        ISerializableComponentBuilder[] entitiesToSerialize { get; }
        Type                            realType            { get; }
    }
}