namespace Svelto.ECS.Internal
{
    sealed class RemoveEntityImplementor : IRemoveEntityComponent
    {
        public RemoveEntityImplementor(IEntityDescriptor descriptor, int groupID) : this(descriptor)
        {
            removeEntityInfo = new RemoveEntityInfo(descriptor, groupID);
        }

        internal RemoveEntityImplementor(IEntityDescriptor descriptor)
        {
            removeEntityInfo = new RemoveEntityInfo(descriptor);
        }

        internal RemoveEntityInfo removeEntityInfo;
    }
}

namespace Svelto.ECS
{
    public interface IRemoveEntityComponent
    {}

    public struct RemoveEntityInfo
    {
        readonly public IEntityDescriptor descriptor;
        readonly public int groupID;
        readonly public bool isInAGroup;

        public RemoveEntityInfo(IEntityDescriptor descriptor) : this()
        {
            this.descriptor = descriptor;
        }

        public RemoveEntityInfo(IEntityDescriptor descriptor, int groupID)
        {
            this.descriptor = descriptor;
            this.groupID = groupID;
            isInAGroup = true;
        }
    }
}
