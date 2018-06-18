using System;
using Svelto.DataStructures;

namespace Svelto.ECS
{
    public interface IEntityDescriptor
    {
        IEntityBuilder[] entitiesToBuild { get; }
    }

    public class EntityDescriptor : IEntityDescriptor
    {
        protected EntityDescriptor(IEntityBuilder[] entityToBuild)
        {
            this.entitiesToBuild = entityToBuild;
        }

        public IEntityBuilder[] entitiesToBuild { get; private set; }
    }

    public static class EntityDescriptorTemplate<TType> where TType : IEntityDescriptor, new()
    {
        public static readonly StaticEntityDescriptorInfo<TType> descriptor = new StaticEntityDescriptorInfo<TType>(new TType());
    }

    public struct DynamicEntityDescriptorInfo<TType> where TType : IEntityDescriptor, new()
    {
        public DynamicEntityDescriptorInfo(FasterList<IEntityBuilder> extraEntityViews) : this()
        {
            DBC.ECS.Check.Require(extraEntityViews.Count > 0,
                          "don't use a DynamicEntityDescriptorInfo if you don't need to use extra EntityViews");

            var defaultEntityViewsToBuild = EntityDescriptorTemplate<TType>.descriptor.entitiesToBuild;
            var length     = defaultEntityViewsToBuild.Length;

            entitiesToBuild = new IEntityBuilder[length + extraEntityViews.Count];

            Array.Copy(defaultEntityViewsToBuild, 0, entitiesToBuild, 0, length);
            Array.Copy(extraEntityViews.ToArrayFast(), 0, entitiesToBuild, length, extraEntityViews.Count);
        }

        public IEntityBuilder[] entitiesToBuild { get; private set; }
    }

    public struct StaticEntityDescriptorInfo<TType> where TType : IEntityDescriptor
    {
        internal StaticEntityDescriptorInfo(TType descriptor) : this()
        {
            entitiesToBuild = descriptor.entitiesToBuild;
        }

        public IEntityBuilder[] entitiesToBuild { get; private set; }
    }
}
