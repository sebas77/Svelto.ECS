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

    public static class EntityDescriptorTemplate<TType> where TType : class, IEntityDescriptor, new()
    {
        public static readonly EntityDescriptorInfo Info = new EntityDescriptorInfo(new TType());
    }

    public struct DynamicEntityDescriptorInfo<TType> where TType : class, IEntityDescriptor, new()
    {
        public readonly IEntityViewBuilder[] entityViewsToBuild;
        
        public DynamicEntityDescriptorInfo(FasterList<IEntityViewBuilder> extraEntityViews)
        {
            DBC.ECS.Check.Require(extraEntityViews.Count > 0,
                          "don't use a DynamicEntityDescriptorInfo if you don't need to use extra EntityViews");

            var defaultEntityViewsToBuild = EntityDescriptorTemplate<TType>.Info.entityViewsToBuild;
            var length     = defaultEntityViewsToBuild.Length;

            entityViewsToBuild = new IEntityViewBuilder[length + extraEntityViews.Count];

            Array.Copy(defaultEntityViewsToBuild, 0, entityViewsToBuild, 0, length);
            Array.Copy(extraEntityViews.ToArrayFast(), 0, entityViewsToBuild, length, extraEntityViews.Count);
        }
    }

    public struct EntityDescriptorInfo
    {
        public readonly IEntityViewBuilder[] entityViewsToBuild;

        internal EntityDescriptorInfo(IEntityDescriptor descriptor)
        {
            entityViewsToBuild = descriptor.entityViewsToBuild;
        }
    }
}
