namespace Svelto.ECS
{
    public interface IEntityDescriptorHolder
    {
        IEntityBuilder[] GetEntitiesToBuild();
    }
}