namespace Svelto.ECS
{
    public interface IEntityDescriptorHolder
    {
        EntityDescriptorInfo<T> RetrieveDescriptorInfo<T>() where T : IEntityDescriptor, new();
    }
}