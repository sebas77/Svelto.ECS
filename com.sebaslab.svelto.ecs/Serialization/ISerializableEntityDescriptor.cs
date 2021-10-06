using System;

namespace Svelto.ECS.Serialization
{
    public interface ISerializableEntityDescriptor : IDynamicEntityDescriptor
    {
        uint                            hash                { get; }
        ISerializableComponentBuilder[] componentsToSerialize { get; }
        Type                            realType            { get; }
    }
}