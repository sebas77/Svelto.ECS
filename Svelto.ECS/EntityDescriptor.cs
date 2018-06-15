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

        public IEntityBuilder[] entitiesToBuild { get; }
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

            var defaultEntityViewsToBuild = EntityDescriptorTemplate<TType>.descriptor.entitiesToBuild;
            var length     = defaultEntityViewsToBuild.Length;

            entitiesToBuild = new IEntityBuilder[length + extraEntityViews.Count];

            Array.Copy(defaultEntityViewsToBuild, 0, entitiesToBuild, 0, length);
            Array.Copy(extraEntityViews.ToArrayFast(), 0, entitiesToBuild, length, extraEntityViews.Count);
        }

        public IEntityBuilder[] entitiesToBuild { get; }
    }

    public struct EntityDescriptor<TType>:IEntityDescriptor where TType : IEntityDescriptor
    {
        internal EntityDescriptor(TType descriptor)
        {
            entitiesToBuild = descriptor.entitiesToBuild;
        }

        public IEntityBuilder[] entitiesToBuild { get; }
    }
}
