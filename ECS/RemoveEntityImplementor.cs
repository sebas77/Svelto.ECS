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

        readonly internal RemoveEntityInfo removeEntityInfo;
        readonly internal int groupID;
        readonly internal bool isInAGroup;
    }
}

namespace Svelto.ECS
{
    public interface IRemoveEntityComponent
    {}

    public struct RemoveEntityInfo
    {
        readonly internal IEntityViewBuilder[] entityViewsToBuild;
        
        public RemoveEntityInfo(IEntityViewBuilder[] entityViews) : this()
        {
            this.entityViewsToBuild = entityViews;
        }
    }
}
