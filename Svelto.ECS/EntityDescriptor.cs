using System;
using DBC;
using Svelto.DataStructures;
using Svelto.ECS.Internal;

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

        public IEntityViewBuilder[] entityViewsToBuild { get; private set;  }
    }

    public interface IEntityDescriptorInfo
    {
    }

    public static class EntityDescriptorTemplate<TType> where TType : IEntityDescriptor, new()
    {
        public static readonly IEntityDescriptorInfo Default = new EntityDescriptorInfo(new TType());
    }

    public class DynamicEntityDescriptorInfo<TType> : EntityDescriptorInfo where TType : IEntityDescriptor, new()
    {
        public DynamicEntityDescriptorInfo(FasterList<IEntityViewBuilder> extraEntityViews)
        {
            Check.Require(extraEntityViews.Count > 0,
                          "don't use a DynamicEntityDescriptorInfo if you don't need to use extra EntityViews");

            var descriptor = new TType();
            var length     = descriptor.entityViewsToBuild.Length;

            entityViewsToBuild = new IEntityViewBuilder[length + extraEntityViews.Count];

            Array.Copy(descriptor.entityViewsToBuild, 0, entityViewsToBuild, 0, length);
            Array.Copy(extraEntityViews.ToArrayFast(), 0, entityViewsToBuild, length, extraEntityViews.Count);

            name = descriptor.ToString();
        }
    }
}

namespace Svelto.ECS.Internal
{
    public class EntityDescriptorInfo : IEntityDescriptorInfo
    {
        internal IEntityViewBuilder[] entityViewsToBuild;
        internal string               name;

        internal EntityDescriptorInfo(IEntityDescriptor descriptor)
        {
            name               = descriptor.ToString();
            entityViewsToBuild = descriptor.entityViewsToBuild;
        }

        protected EntityDescriptorInfo()
        {
        }
    }
}