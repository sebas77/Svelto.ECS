using System;

namespace Svelto.ECS
{
    public interface IEntityDescriptor
    {
        IComponentBuilder[] componentsToBuild { get; }
    }
    
    public interface IDynamicEntityDescriptor: IEntityDescriptor
    {
    }

    static class EntityDescriptorTemplate<TType> where TType : IEntityDescriptor, new()
    {
        static EntityDescriptorTemplate()
        {
            realDescriptor = new TType();
            descriptor     = realDescriptor;
        }

        public static TType             realDescriptor { get; }
        public static Type              type           => typeof(TType);
        public static IEntityDescriptor descriptor     { get; }
    }
}
