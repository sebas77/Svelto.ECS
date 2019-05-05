namespace Svelto.ECS
{
    public interface IEntityDescriptor
    {
        IEntityBuilder[] entitiesToBuild { get; }
    }

    static class EntityDescriptorTemplate<TType> where TType : IEntityDescriptor, new()
    {
        internal static readonly StaticEntityDescriptorInfo<TType> descriptor 
            = new StaticEntityDescriptorInfo<TType>(new TType());
    }
}
