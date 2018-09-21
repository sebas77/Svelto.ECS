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

    public class StaticEntityDescriptorInfo<TType>: IEntityDescriptor where TType : IEntityDescriptor
    {
        internal StaticEntityDescriptorInfo(TType descriptor)
        {
            entitiesToBuild = descriptor.entitiesToBuild;
        }

        public IEntityBuilder[] entitiesToBuild { get; private set; }
    }
}
