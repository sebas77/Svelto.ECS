namespace Svelto.ECS.Hybrid
{
    public interface IEntityDescriptorHolder
    {
        IEntityDescriptor GetDescriptor();

        string groupName { get; }
        ushort id { get; }
    }
}