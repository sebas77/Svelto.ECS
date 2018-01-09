namespace Svelto.ECS.Internal
{
    sealed class RemoveEntityImplementor : IRemoveEntityComponent
    {
        public RemoveEntityImplementor(IEntityViewBuilder[] entityViews, int groupID):this(entityViews)
        {
            this.groupID = groupID;
            isInAGroup = true;
        }

        internal RemoveEntityImplementor(IEntityViewBuilder[] entityViews)
        {
            removeEntityInfo = new RemoveEntityInfo(entityViews);
        }

        internal readonly RemoveEntityInfo removeEntityInfo;
        internal readonly int groupID;
        internal readonly bool isInAGroup;
    }
}

namespace Svelto.ECS
{
    public interface IRemoveEntityComponent
    {}

    public struct RemoveEntityInfo
    {
        internal readonly IEntityViewBuilder[] entityViewsToBuild;
        
        public RemoveEntityInfo(IEntityViewBuilder[] entityViews) : this()
        {
            this.entityViewsToBuild = entityViews;
        }
    }
}
