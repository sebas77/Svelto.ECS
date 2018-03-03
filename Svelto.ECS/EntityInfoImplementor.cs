namespace Svelto.ECS.Internal
{
    class EntityInfoView : EntityView
    {
        internal IEntityViewBuilder[] entityViews;
        internal int                  groupID;
        internal bool                 isInAGroup;
    }
}