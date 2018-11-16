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
            entitiesToBuild = entityToBuild;
        }
        
        public IEntityBuilder[] entitiesToBuild { get; }
    }

    static class EntityDescriptorTemplate<TType> where TType : IEntityDescriptor, new()
    {
        public static readonly StaticEntityDescriptorInfo<TType> descriptor 
            = new StaticEntityDescriptorInfo<TType>(new TType());
    }
}
