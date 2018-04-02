using System;
using DBC;
using Svelto.DataStructures;

namespace Svelto.ECS
{
    public interface IEntityDescriptor
    {
        IEntityViewBuilder[] entityViewsToBuild { get; }
    }

    public class EntityDescriptor : IEntityDescriptor
    {
        protected EntityDescriptor(IEntityViewBuilder[] entityViewsToBuild)
        {
            this.entityViewsToBuild = entityViewsToBuild;
        }

        public IEntityViewBuilder[] entityViewsToBuild { get; }
    }

    public static class EntityDescriptorTemplate<TType> where TType : IEntityDescriptor, new()
    {
        public static readonly EntityDescriptorInfo Default = new EntityDescriptorInfo(new TType());
    }

    public class DynamicEntityDescriptorInfo<TType> : EntityDescriptorInfo where TType : IEntityDescriptor, new()
    {
        public DynamicEntityDescriptorInfo(FasterList<IEntityViewBuilder> extraEntityViews)
        {
            Check.Require(extraEntityViews.Count > 0,
                          "don't use a DynamicEntityDescriptorInfo if you don't need to use extra EntityViews");

            var defaultEntityViewsToBuild = EntityDescriptorTemplate<TType>.Default.entityViewsToBuild;
            var length     = defaultEntityViewsToBuild.Length;

            entityViewsToBuild = new IEntityViewBuilder[length + extraEntityViews.Count];

            Array.Copy(defaultEntityViewsToBuild, 0, entityViewsToBuild, 0, length);
            Array.Copy(extraEntityViews.ToArrayFast(), 0, entityViewsToBuild, length, extraEntityViews.Count);

            name = EntityDescriptorTemplate<TType>.Default.name;
        }
    }

    public class EntityDescriptorInfo
    {
        internal IEntityViewBuilder[] entityViewsToBuild;
        internal string name;

        internal EntityDescriptorInfo(IEntityDescriptor descriptor)
        {
            name = descriptor.ToString();
            entityViewsToBuild = descriptor.entityViewsToBuild;
        }

        protected EntityDescriptorInfo()
        { }
    }
}
