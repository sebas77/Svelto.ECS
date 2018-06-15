using System;
using Svelto.DataStructures;

namespace Svelto.ECS
{
    public interface IEntityDescriptor
    {
        IEntityBuilder[] EntityToBuild { get; }
    }

    public class EntityDescriptor : IEntityDescriptor
    {
        protected EntityDescriptor(IEntityBuilder[] entityToBuild)
        {
            this.EntityToBuild = entityToBuild;
        }

        public IEntityBuilder[] EntityToBuild { get; }
    }

    public static class EntityDescriptorTemplate<TType> where TType : IEntityDescriptor, new()
    {
        public static readonly EntityDescriptor<TType> descriptor = new EntityDescriptor<TType>(new TType());
    }

    public struct DynamicEntityDescriptorInfo<TType>:IEntityDescriptor where TType : IEntityDescriptor, new()
    {
        public DynamicEntityDescriptorInfo(FasterList<IEntityBuilder> extraEntityViews)
        {
            DBC.ECS.Check.Require(extraEntityViews.Count > 0,
                          "don't use a DynamicEntityDescriptorInfo if you don't need to use extra EntityViews");

            var defaultEntityViewsToBuild = EntityDescriptorTemplate<TType>.descriptor.EntityToBuild;
            var length     = defaultEntityViewsToBuild.Length;

            EntityToBuild = new IEntityBuilder[length + extraEntityViews.Count];

            Array.Copy(defaultEntityViewsToBuild, 0, EntityToBuild, 0, length);
            Array.Copy(extraEntityViews.ToArrayFast(), 0, EntityToBuild, length, extraEntityViews.Count);
        }

        public IEntityBuilder[] EntityToBuild { get; }
    }

    public struct EntityDescriptor<TType>:IEntityDescriptor where TType : IEntityDescriptor
    {
        internal EntityDescriptor(TType descriptor)
        {
            EntityToBuild = descriptor.EntityToBuild;
        }

        public IEntityBuilder[] EntityToBuild { get; }
    }
}
